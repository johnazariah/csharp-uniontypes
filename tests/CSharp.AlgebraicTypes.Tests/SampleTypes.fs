namespace CSharp.AlgebraicTypes.Tests

open CSharp.AlgebraicTypes

[<AutoOpen>]
module SampleTypes =
    let internal Maybe_T =
        {
            BaseType = None
            UnionTypeName = UnionTypeName "Maybe"
            UnionTypeParameters = [ TypeArgument "T" ]
            UnionMembers =
                [
                    { MemberName = UnionMemberName "None"; MemberArgumentType = None }
                    {
                        MemberName = UnionMemberName "Some"
                        MemberArgumentType = Some { FullyQualifiedTypeName = "T"; TypeArguments = [] }
                    }
                ]
        }

    let internal SingleValue_T =
        {
            BaseType = (("Maybe", []), Some [FullTypeName.apply(("T", []), None)]) |> (FullTypeName.apply >> Some)
            UnionTypeName = UnionTypeName "SingleValue"
            UnionTypeParameters = [ TypeArgument "T" ]
            UnionMembers =
                [
                    {
                        MemberName = UnionMemberName "Some"
                        MemberArgumentType = Some { FullyQualifiedTypeName = "T"; TypeArguments = [] }
                    }
                ]
        }

    let internal TrafficLights =
        {
            BaseType = None
            UnionTypeName = UnionTypeName "TrafficLights"
            UnionTypeParameters = []
            UnionMembers =
                [
                    { MemberName = UnionMemberName "Red"; MemberArgumentType = None }
                    { MemberName = UnionMemberName "Amber"; MemberArgumentType = None }
                    { MemberName = UnionMemberName "Green"; MemberArgumentType = None }
                ]
        }

    let internal TrafficLightsToStopFor =
        {
            BaseType = (("TrafficLights", []), None) |> (FullTypeName.apply >> Some)
            UnionTypeName = UnionTypeName "TrafficLightsToStopFor"
            UnionTypeParameters = []
            UnionMembers =
                [
                    { MemberName = UnionMemberName "Red"; MemberArgumentType = None }
                    { MemberName = UnionMemberName "Amber"; MemberArgumentType = None }
                ]
        }