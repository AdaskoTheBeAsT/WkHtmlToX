# 0012 - Accept `byte[]` and `Stream` as HTML input

- Status: Accepted
- Date: 2020-03-28
- Traceability: `84927e3` "introduced ability to pass byte array and stream containing html to convert",
  `1af3ebc` "change interface"

## Context

Original API accepted only `string`. Web hosts typically already have
HTML as `byte[]` or buffered `Stream` coming from Razor, a template
engine or a reverse proxy. Forcing a `string` intermediate caused an
extra UTF-16 copy and lost byte-order-mark handling.

## Decision

Extend `IHtmlToPdfDocument` / `IHtmlToImageDocument` to carry either
`string`, `byte[]` or `Stream` HTML content. The converter picks the
most efficient path per input type and routes it through the pool
introduced by ADR 0011.

## Consequences

- Public surface grows but stays backward compatible.
- `Stream` inputs are read once (non-seekable streams allowed) into a
  pooled buffer then passed to the native side.
- A `HtmlContentStreamTooLargeException` protects against unbounded
  server allocations.
