﻿namespace Shard.EnzoSamy.Api;

internal static class Extensions
{
    internal static Guid NextGuid(this Random random)
    {
        var bytes = new byte[16];
        random.NextBytes(bytes);

        return new Guid(bytes);
    }
}