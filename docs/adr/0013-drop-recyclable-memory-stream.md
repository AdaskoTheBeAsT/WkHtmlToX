# 0013 - Drop `RecyclableMemoryStream` dependency

- Status: Accepted. Supersedes [ADR 0003](0003-recyclable-memory-stream-and-no-unsafe.md)
- Date: 2020-03-29
- Traceability: `9579d8d` "removed dependency from RecyclableMemoryStream, fixed tests, changed samples"

## Context

After ADR 0011 introduced `ArrayPool<byte>.Shared`, the only remaining
use of `RecyclableMemoryStream` was wrapping the pooled buffer as a
`Stream`. That was no longer pulling its weight: an extra dependency,
an extra allocation and a pool-inside-a-pool.

## Decision

Remove the `Microsoft.IO.RecyclableMemoryStream` dependency. Operate
directly on the rented `byte[]` / `Span<byte>`. Use `MemoryStream`
over the rented segment only where a `Stream` is mandated by a public
contract.

## Consequences

- One less transitive dependency and one less `ObjectPool` instance.
- All buffer accounting lives in `ArrayPool`, which is simpler to
  reason about.
- Requires careful `try/finally` discipline, verified by memory tests.
