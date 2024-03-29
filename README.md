# Pingu

[![Join the chat at https://gitter.im/pingu-png/Lobby](https://badges.gitter.im/pingu-png/Lobby.svg)](https://gitter.im/pingu-png/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![Build status](https://teamcity.coderinserepeat.com/app/rest/builds/buildType:(id:Pingu_Build)/statusIcon)](https://teamcity.coderinserepeat.com/viewType.html?buildTypeId=Pingu_Build&guest=1)

Pingu is a fully managed PNG encoder, written in C#.

## Goals

Pingu's goal is to be a full-featured PNG encoder, written
entirely in C#, with as few external dependencies as possible.

It intends to produce images that pass [pngcheck][check], and that
validate in all of the most common encoders. Performance, while important,
is a secondary concern for Pingu. Code will prefer to be clean rather than
fast, unless it can balance both.

## License

Pingu is licensed under the MIT license. See [LICENSE.md][license].

[check]: http://www.libpng.org/pub/png/apps/pngcheck.html
[license]: LICENSE.md