namespace BrightSword.CSharpExtensions.DiscriminatedUnion

module internal CodeGenerator =
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax

    open BrightSword.RoslynWrapper.Common
    open BrightSword.RoslynWrapper.FieldDeclaration
    open BrightSword.RoslynWrapper.MethodDeclaration
    open BrightSword.RoslynWrapper.PropertyDeclaration
    open BrightSword.RoslynWrapper.ConstructorDeclaration
    open BrightSword.RoslynWrapper.ClassDeclaration
    open BrightSword.RoslynWrapper.ObjectCreation
    open BrightSword.RoslynWrapper.Conversion
    open BrightSword.RoslynWrapper.NamespaceDeclaration
    open BrightSword.RoslynWrapper.CompilationUnit
    open BrightSword.RoslynWrapper.CodeGenerator

    type SF = SyntaxFactory

    let to_private_ctor du =
        let className = du.UnionTypeName.unapply
        seq {
            yield ``constructor`` className ``(`` [] ``)``
                    ``:`` []
                    [``private``]
                    ``{``
                        []
                    ``}``
                :> MemberDeclarationSyntax
        }
          
    let to_match_function_parameter_name = 
        toParameterName >> sprintf "%sFunc"

    let to_match_function_parameters result_type du =        
        let to_match_function_parameter um =
            let match_function_parameter_type = 
                um.MemberArgumentType 
                |> Option.fold (fun _ t -> sprintf "Func<%s, %s>" t.unapply result_type) (sprintf "Func<%s>" result_type)

            let match_function_parameter_name =
                um.MemberName.unapply
                |> to_match_function_parameter_name
            in 
            (match_function_parameter_name, match_function_parameter_type)
        
        in
        du.UnionMembers |> Seq.map to_match_function_parameter
    
    let to_match_function invocation du =
        let parameters = 
            to_match_function_parameters "TResult" du
        in
        seq {
            yield ``method`` "TResult" "Match" ``<<`` [ "TResult" ] ``>>`` ``(`` parameters ``)``
                [``public``; (invocation |> Option.fold (fun _ s -> ``override``) ``abstract``)]
                ``=>`` invocation
            :> MemberDeclarationSyntax
        }

    //<code>
    //   // match_function_abstract
    //   public abstract R Match{R}(Func{R} noneFunc, Func{T, R} someFunc);
    //                   
    //</code>
    let to_match_function_abstract = 
        to_match_function None
    
    //<code>
    //  // access_member
    //  public static readonly Maybe{T} None = new ChoiceTypes.None();
    //  public static Maybe{T} NewSome(T value) => new ChoiceTypes.Some(value);
    //</code>
    let to_access_members (du : UnionType) =
        let union_name = du.unapply
        
        let to_access_member um = 
            let member_name = um.MemberName.unapply
            
            let to_access_member_method _ (t : FullTypeName) =
                let method_name = member_name |> sprintf "New%s"
                let method_arg_name = "value"
                let method_arg_type = t.unapply
                let initialization_expression = 
                    ``=>`` (``new`` ["ChoiceTypes"; member_name ]  ``(`` [ method_arg_name ]  ``)``)
                in
                ``method`` union_name method_name ``<<`` [] ``>>`` ``(`` [ (method_arg_name, method_arg_type) ] ``)``
                    [``public``; ``static``]
                    ``=>`` (Some initialization_expression)
                :> MemberDeclarationSyntax

            let to_access_member_field =
                let field_initializer = 
                    ``:=`` (``new`` ["ChoiceTypes"; member_name] ``(`` [] ``)``)
                in
                ``field`` union_name member_name
                    [``public``; ``static``; ``readonly``]
                    ``:=`` (Some field_initializer)
                :> MemberDeclarationSyntax
            in
            um.MemberArgumentType
            |> Option.fold to_access_member_method to_access_member_field

        in
        du.UnionMembers 
        |> Seq.map to_access_member

    //  // wrapper_type
    //  private static class ChoiceTypes
    //  {
    //      // choice_class (value)
    //      public class Some : Maybe{T}
    //      {
    //         ...
    //      }
    //  
    //      // choice_class (singleton)
    //      public class None : Maybe{T}
    //      {
    //         ...
    //      }
    //  }
    let to_wrapper_type (du : UnionType) =
        let union_name = du.unapply

        let to_choice_class um = 
            let member_name = um.MemberName.unapply
            
            let match_function_name =
                um.MemberName.unapply
                |> to_match_function_parameter_name

            let match_function_override (argument_names : string list) =
                let invocation =
                    let invocation_arguments = 
                        argument_names
                        |> List.map (SF.IdentifierName >> SF.Argument)
                        |> (SF.SeparatedList >> SF.ArgumentList)
                    in
                    match_function_name
                    |> (toIdentifierName >> SF.InvocationExpression)
                    |> (fun ie -> ie.WithArgumentList invocation_arguments)
                    |> SF.ArrowExpressionClause
                in
                du |> to_match_function (Some invocation)

            //  // choice_class (value)
            //  public class Some : Maybe{T}
            //  {
            //      // choice_class_ctor
            //      public Some(T value)
            //      {
            //          Value = value;
            //      }
            //  
            //      // value_property
            //      public T Value { get; }
            //  
            //      // match_function_override
            //      public override R Match{R}(Func{R} noneFunc, Func{T, R} someFunc) => 
            //          someFunc(Value);
            //  
            //      // Value Semantics (https://msdn.microsoft.com/en-us/library/dd183755.aspx)
            //      // equals_object_override
            //      public override bool Equals(object other) =>
            //          this.Equals(other as Some{T});
            //  
            //      // equals_implementation
            //      public bool Equals(Some{T} other) =>
            //          (Object.ReferenceEquals(this, other)
            //          || (!Object.ReferenceEquals(this, null)
            //              && (this.GetType() == other.GetType())
            //              && (Value == other.Value)));
            //  
            //      // hashcode_override
            //      public override int GetHashCode() =>
            //          Value.GetHashCode();
            //  
            //      // eq_operator
            //      public static bool operator ==(Some{T} left, Some{T} right) =>
            //          (((left == null) && (right == null))
            //          || ((left != null) && (left.Equals(right))));
            //  
            //      // neq_operator
            //      public static bool operator !=(Some{T} left, Some{T} right) =>
            //          (!(left == right));
            //  }            
            let to_choice_class_value_members _ (t : FullTypeName) =
                let arg_type_name = t.unapply
                let ctor = 
                    let assignment = ("Value" <-- "value") |> SF.ExpressionStatement :> StatementSyntax
                    in
                    ``constructor`` member_name ``(`` [("value", arg_type_name)] ``)``
                        ``:`` []
                        [``public``]
                        ``{``
                            [ assignment ]
                        ``}``
                    :> MemberDeclarationSyntax

                let value_property =
                    ``propg`` arg_type_name "Value" [``private``]
                    :> MemberDeclarationSyntax
                in
                seq {
                    yield ctor
                    yield value_property
                    yield! (match_function_override ["Value"])
//                        yield! equals_object_override
//                        yield! equals_implementation
//                        yield! hashcode_override
//                        yield! eq_operator
//                        yield! neq_operator
                }

            //  // choice_class (singleton)
            //  public class None : Maybe{T}
            //  {
            //      // - no ctor for singleton -
            //  
            //      // match_function_override
            //      public override R Match{R}(Func{R} noneFunc, Func{T, R} someFunc) => 
            //          noneFunc();
            //  
            //      // equals_object_override
            //      public override bool Equals(object other) =>
            //          this.Equals(other as None);
            //  
            //      // equals_implementation
            //      public bool Equals(None other) => true;
            //  
            //      // hashcode_override
            //      public override int GetHashCode() =>
            //          this.GetType().FullName.GetHashCode();
            //  
            //      // eq_operator
            //      public static bool operator ==(None left, None right) => true;
            //  
            //      // neq_operator
            //      public static bool operator !=(None left, None right) => false;
            //  }
            let to_choice_class_singleton_members =
                seq {
                    yield! (match_function_override [])
//                        yield! equals_object_override
//                        yield! equals_implementation
//                        yield! hashcode_override
//                        yield! eq_operator
//                        yield! neq_operator
                }

            let members = 
                um.MemberArgumentType
                |> Option.fold to_choice_class_value_members to_choice_class_singleton_members
            in 
            ``class`` member_name ``<<`` [] ``>>``
                ``:`` (Some union_name) ``,`` []
                [``public``]
                ``{``
                        members
                ``}``
                :> MemberDeclarationSyntax

        let choice_classes = 
            du.UnionMembers |> Seq.map to_choice_class
        in
        seq {
            yield ``class``  "ChoiceTypes" ``<<`` [] ``>>``
                ``:`` None ``,`` []
                [``private``; ``static``]
                ``{``
                    choice_classes
                ``}``
            :> MemberDeclarationSyntax
        }

    let build_class_declaration_syntax fns du = 
        let class_name = du.UnionTypeName.unapply

        let type_parameters =
            du.UnionTypeParameters
            |> Seq.map (fun p -> p.unapply)
        
        let members = fns |> Seq.collect (fun f -> du |> f)
        in
        ``class`` class_name ``<<`` type_parameters ``>>``
            ``:`` None ``,`` []
            [``public``; ``abstract``; ``partial``]
            ``{``
                members
            ``}``

    // <summary>
    // Entry-point to the C# DU generator.
    //
    // Generates the Roslyn <code>ClassDeclarationSyntax</code> for a given a UnionType AST. 
    //
    // Given 
    // <code lang="csharp">
    //     union Maybe{T} 
    //     {
    //         Some{T};
    //         None;
    //     }
    // </code>
    //
    // this function generates the syntax to define a class that looks like:
    //
    // <code lang="csharp">
    //    // class_declaration
    //    public abstract class Maybe{T}
    //    {
    //        // private_ctor
    //        private Maybe() {}
    //
    //        // match_function_abstract
    //        public abstract R Match{R}(Func{R} noneFunc, Func{T, R} someFunc);
    //
    //        // access_member
    //        public static readonly Maybe{T} None = new ChoiceTypes.None();
    //        public static Maybe{T} NewSome(T value) => new ChoiceTypes.Some(value);
    //
    //        // wrapper_type
    //        private static class ChoiceTypes
    //        {
    //            // choice_class (value)
    //            public class Some : Maybe{T}
    //            {
    //                // choice_class_ctor
    //                public Some(T value)
    //                {
    //                    Value = value;
    //                }
    //
    //                // value_property
    //                public T Value { get; }
    //
    //                // match_function_override
    //                public override R Match{R}(Func{R} noneFunc, Func{T, R} someFunc) => 
    //                    someFunc(Value);
    //
    //                // Value Semantics (https://msdn.microsoft.com/en-us/library/dd183755.aspx)
    //                // equals_object_override
    //                public override bool Equals(object other) =>
    //                    this.Equals(other as Some{T});
    //
    //                // equals_implementation
    //                public bool Equals(Some{T} other) =>
    //                    (Object.ReferenceEquals(this, other)
    //                    || (!Object.ReferenceEquals(this, null)
    //                        && (this.GetType() == other.GetType())
    //                        && (Value == other.Value)));
    //
    //                // hashcode_override
    //                public override int GetHashCode() =>
    //                    Value.GetHashCode();
    //
    //                // eq_operator
    //                public static bool operator ==(Some{T} left, Some{T} right) =>
    //                    (((left == null) && (right == null))
    //                    || ((left != null) && (left.Equals(right))));
    //
    //                // neq_operator
    //                public static bool operator !=(Some{T} left, Some{T} right) =>
    //                    (!(left == right));
    //            }
    //
    //            // choice_class (singleton)
    //            public class None : Maybe{T}
    //            {
    //                // choice_class_ctor
    //                public None {}
    //
    //                // match_function_override
    //                public override R Match{R}(Func{R} noneFunc, Func{T, R} someFunc) => 
    //                    noneFunc(Value);
    //
    //                // equals_object_override
    //                public override bool Equals(object other) =>
    //                    this.Equals(other as None);
    //
    //                // equals_implementation
    //                public bool Equals(None other) => true;
    //
    //                // hashcode_override
    //                public override int GetHashCode() =>
    //                    this.GetType().FullName.GetHashCode();
    //
    //                // eq_operator
    //                public static bool operator ==(None left, None right) => true;
    //
    //                // neq_operator
    //                public static bool operator !=(None left, None right) => false;
    //            }
    //        }
    //    }
    // </code>
    // 
    // </summary>
    // <param name="du">The UnionType AST to generate code for</param>
    let to_class_declaration du =
        let fns =
            [
                to_private_ctor
                to_match_function_abstract
                to_access_members
                to_wrapper_type
            ]
        in
        build_class_declaration_syntax fns du

    let to_namespace_declaration ns = 
        ``namespace`` ns.NamespaceName.unapply
            ``{``
                ns.
            ``}``