namespace CSharp.AlgebraicTypes.Tests

open CSharp.AlgebraicTypes

[<AutoOpen>]
module SampleTypes =
    let internal Maybe_T =
        {
            BaseType = None
            TypeDeclaration = { TypeName = Symbol "Maybe"; TypeArguments = Some ([ Symbol "T" ]) }
            TypeMembers =
                [
                    UnionTypeMember.UntypedMember (Symbol "None")
                    UnionTypeMember.TypedMember ({MemberName = Symbol "Some"; MemberType = {TypeName = DottedName "T"; TypeParameters = None}})
                ]
        }

    let internal SingleValue_T =
        {
            BaseType = Some ({TypeName = DottedName "Maybe"; TypeParameters = Some[ {TypeName = DottedName "T"; TypeParameters = None}]})
            TypeDeclaration = { TypeName = Symbol "SingleValue"; TypeArguments = Some ([ Symbol "T" ]) }
            TypeMembers =
                [
                    UnionTypeMember.TypedMember ({MemberName = Symbol "Some"; MemberType = {TypeName = DottedName "T"; TypeParameters = None}})
                ]
        }

    let internal TrafficLights =
        {
            BaseType = None
            TypeDeclaration = { TypeName = Symbol "TrafficLights"; TypeArguments = None }
            TypeMembers =
                [
                    UnionTypeMember.UntypedMember (Symbol "Red")
                    UnionTypeMember.UntypedMember (Symbol "Amber")
                    UnionTypeMember.UntypedMember (Symbol "Green")
                ]
        }

    let internal TrafficLightsToStopFor =
        {
            BaseType = Some ({TypeName = DottedName "TrafficLights"; TypeParameters = None})
            TypeDeclaration = { TypeName = Symbol "TrafficLightsToStopFor"; TypeArguments = None }
            TypeMembers =
                [
                    UnionTypeMember.UntypedMember (Symbol "Red")
                    UnionTypeMember.UntypedMember (Symbol "Amber")
                ]
        }