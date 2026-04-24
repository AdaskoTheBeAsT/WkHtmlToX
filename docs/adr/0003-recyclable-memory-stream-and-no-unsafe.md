# 0003 - Use `RecyclableMemoryStream` and remove `unsafe` code

- Status: Superseded by [ADR 0013](0013-drop-recyclable-memory-stream.md)
- Date: 2019-12-08
- Traceability: `d16f171` "remove unsafe use RecyclableMemoryStream"

## Context

The original prototype used `unsafe` blocks and raw pointer
arithmetic to pass HTML to the native side. That made the code
hard to audit, prevented `AllowPartiallyTrustedCallers` scenarios,
and required `<AllowUnsafeBlocks>` in the csproj.

## Decision

Replace hand-rolled pointer code with
`Microsoft.IO.RecyclableMemoryStream` and safe Span/array-based
marshaling. Keep the public API `byte[]`-oriented.

## Consequences

- Removes the need for `unsafe`; simplifies analyzers and audit.
- Adds a third-party dependency (`Microsoft.IO.RecyclableMemoryStream`).
- Reduces GC pressure versus naive `MemoryStream` use.
- Later walked back by ADR 0013 once `ArrayPool<byte>.Shared` (ADR 0011)
  proved to be enough.
