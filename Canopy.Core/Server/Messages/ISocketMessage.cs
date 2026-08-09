// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Canopy.Server.Messages;

public interface ISocketMessage
{
    string Encode();

    ISocketMessage Decode(string message);
}
