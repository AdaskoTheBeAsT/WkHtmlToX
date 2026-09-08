# 0037 - Reserve aggregate inline input capacity before copying

- Status: Accepted
- Date: 2026-09-07
- Extends: [0036](0036-request-ownership-and-native-binding.md).

## Context

A per-request input limit and an admission count still allow their product in
retained input. The defaults previously admitted up to 1 GiB of inline PDF
payload, before native allocations, settings, and output. Allocating snapshots
before checking aggregate capacity would make rejection too late.

## Decision

- Add `MaxBufferedInputBytes`, default 128 MiB per engine, validated to cover
  `MaxInputBytes`. Keep the 32 MiB per-PDF limit and independent output budget.
- Snapshot settings and payload references, validate the complete PDF, and
  reserve its aggregate payload under the admission lock before cloning any
  byte arrays or allocating stream buffers.
- Count encoded HTML bytes using the captured encoding, byte-array lengths,
  and remaining readable/seekable stream lengths. Page/Xsl and image paths
  reserve no inline payload. This is a logical payload budget, not a heap meter.
- Reject aggregate exhaustion immediately as `ResourceLimit`, with no native
  initialization or waiting queue. Invalid per-request size remains `InvalidInput`.
- Hold the reservation through input preparation, native execution, and output
  delivery. Release on every terminal path, including snapshot failure, I/O
  failure, cancellation, worker rejection, and shutdown.
- Revalidate stream input against the admission reservation before allocation.
  Growth cannot silently increase reserved capacity; input preparation fails.
  Streams remain borrowed and must not be mutated during a request.

## Consequences

Memory admission is independent of the request-count limit. Slow destinations
retain both their input reservation and detached output capacity until completion.
The engine deliberately does not pool/spool input or claim an exact working-set
ceiling: caller-held objects, UTF-16 strings, settings, terminators, temporary
native-marshalling copies/pools, native memory, and remote resources are excluded.
No throughput or long-running memory-stability claim follows from this change.
