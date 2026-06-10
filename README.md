# RICADO.Omron
An Omron PLC Communication Library for .NET 8+ Applications

## Starter Example

Install the package, add `using RICADO.Omron;`, then create and initialize an `OmronPLC` before making read or write requests.

```csharp
using RICADO.Omron;

using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

// Local and remote node IDs must match your FINS network settings.
using OmronPLC plc = new OmronPLC(
    localNodeId: 1,
    remoteNodeId: 10,
    connectionMethod: enConnectionMethod.TCP,
    remoteHost: "192.168.0.10",
    port: 9600);

await plc.InitializeAsync(cts.Token);

// Read one bit from Work bit area W100.00.
ReadBitsResult readBit = await plc.ReadBitAsync(
    address: 100,
    bitIndex: 0,
    dataType: enMemoryBitDataType.Work,
    cancellationToken: cts.Token);

bool bitValue = readBit.Values[0];

// Write one bit to Work bit area W100.00.
await plc.WriteBitAsync(
    value: true,
    address: 100,
    bitIndex: 0,
    dataType: enMemoryBitDataType.Work,
    cancellationToken: cts.Token);

// Read two words from Data Memory D200-D201.
ReadWordsResult readWords = await plc.ReadWordsAsync(
    startAddress: 200,
    length: 2,
    dataType: enMemoryWordDataType.DataMemory,
    cancellationToken: cts.Token);

short firstWord = readWords.Values[0];
short secondWord = readWords.Values[1];

// Write two words to Data Memory D200-D201.
await plc.WriteWordsAsync(
    values: new short[] { 123, 456 },
    startAddress: 200,
    dataType: enMemoryWordDataType.DataMemory,
    cancellationToken: cts.Token);
```
