using System;
using Blauhaus.Errors;

namespace Blauhaus.Orleans.Abstractions.Errors;

public static class GrainError
{
    public static Error ActivationFailed = Error.Create("Grain activation failed ");
    public static Error InvalidIdentity = Error.Create("Grain identity is not valid for this grain type");
}