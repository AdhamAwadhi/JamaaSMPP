# ShortMessage Documentation

## Table of Contents
1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Class Structure](#class-structure)
4. [Message Types](#message-types)
5. [Properties](#properties)
6. [Methods](#methods)
7. [Message Processing](#message-processing)
8. [Multi-Part Messages](#multi-part-messages)
9. [Usage Examples](#usage-examples)
10. [Best Practices](#best-practices)

## Overview

The `ShortMessage` class hierarchy provides a comprehensive framework for handling SMS messages in the SMPP library. It includes support for simple text messages, multi-part concatenated messages, and delivery receipts, with automatic PDU generation and message splitting capabilities.

### Key Responsibilities
- **Message Representation**: Provides structured representation of SMS messages
- **PDU Generation**: Converts messages to SMPP PDUs for transmission
- **Multi-Part Support**: Handles concatenated messages with UDH (User Data Header)
- **Delivery Tracking**: Manages delivery receipts and message states
- **Address Management**: Handles source and destination addresses
- **Message Splitting**: Automatically splits long messages into multiple parts

## Architecture

The ShortMessage hierarchy operates as a bridge between high-level message objects and low-level SMPP PDUs:

```mermaid
graph TB
    subgraph "Message Layer"
        SM[ShortMessage]
        TM[TextMessage]
        MPTM[MultiPartTextMessage]
    end
    
    subgraph "PDU Layer"
        SSM[SubmitSm]
        DSM[DeliverSm]
        SSMR[SubmitSmResp]
        DSMR[DeliverSmResp]
    end
    
    subgraph "SMPP Protocol"
        UDH[User Data Header]
        SEG[Message Segments]
        SEQ[Sequence Numbers]
    end
    
    SM --> TM
    SM --> MPTM
    TM --> SSM
    TM --> DSM
    MPTM --> SSM
    MPTM --> DSM
    SSM --> SSMR
    DSM --> DSMR
    MPTM --> UDH
    UDH --> SEG
    UDH --> SEQ
```

## Class Structure

### Class Hierarchy
```mermaid
classDiagram
    class ShortMessage {
        <<abstract>>
        #string vSourceAddress
        #string vDestinatinoAddress
        #int vMessageCount
        #int vSegmentID
        #int vSequenceNumber
        #bool vRegisterDeliveryNotification
        #string vReceiptedMessageId
        #string vUserMessageReference
        #bool vSubmitUserMessageReference
        #bool vSubmitReceiptedMessageId
        #MessageState? vMessageState
        #byte[] vNetworkErrorCode
        +SourceAddress string
        +DestinationAddress string
        +MessageCount int
        +SegmentID int
        +SequenceNumber int
        +RegisterDeliveryNotification bool
        +ReceiptedMessageId string
        +UserMessageReference string
        +SubmitUserMessageReference bool
        +SubmitReceiptedMessageId bool
        +MessageState MessageState?
        +NetworkErrorCode byte[]
        +GetMessagePDUs() IEnumerable~SendSmPDU~
        #GetPDUs() IEnumerable~SendSmPDU~*
    }
    
    class TextMessage {
        +string Text
        +ConcatenationType ConcatenationType
        +GetPDUs() IEnumerable~SendSmPDU~
    }
    
    class MultiPartTextMessage {
        +string Text
        +ConcatenationType ConcatenationType
        +GetPDUs() IEnumerable~SendSmPDU~
    }
    
    ShortMessage <|-- TextMessage
    ShortMessage <|-- MultiPartTextMessage
```

### Core Dependencies
- **SendSmPDU**: SMPP SubmitSm PDU for message transmission
- **SmppAddress**: Address representation for source and destination
- **DataCoding**: Character encoding specification
- **SmppEncodingService**: Encoding/decoding service
- **Udh**: User Data Header for concatenated messages

## Message Types

### TextMessage
**Purpose**: Simple text messages (single or automatically split)
**Characteristics**:
- Single text property
- Automatic message splitting
- Standard SMPP encoding support
- Delivery notification support

### MultiPartTextMessage
**Purpose**: Explicitly controlled multi-part messages
**Characteristics**:
- Manual control over splitting
- Custom UDH generation
- Segment management
- Sequence number control

### Message Type Selection
```mermaid
flowchart TD
    A[Message to Send] --> B{Message Length}
    B -->|<= 160 chars| C[TextMessage]
    B -->|> 160 chars| D{Control Preference}
    D -->|Automatic| E[TextMessage with Auto-Split]
    D -->|Manual| F[MultiPartTextMessage]
    
    C --> G[Single PDU]
    E --> H[Multiple PDUs with UDH]
    F --> I[Multiple PDUs with Custom UDH]
```

## Properties

### Address Properties
| Property | Type | Description |
|----------|------|-------------|
| `SourceAddress` | `string` | Sender's phone number or short code |
| `DestinationAddress` | `string` | Recipient's phone number |

### Message Properties
| Property | Type | Description |
|----------|------|-------------|
| `MessageCount` | `int` | Total number of segments in concatenated message |
| `SegmentID` | `int` | Index of this segment (0-based) |
| `SequenceNumber` | `int` | Sequence number for concatenated message |

### Delivery Properties
| Property | Type | Description |
|----------|------|-------------|
| `RegisterDeliveryNotification` | `bool` | Request delivery receipt |
| `ReceiptedMessageId` | `string` | SMSC-assigned message ID |
| `UserMessageReference` | `string` | User-defined message reference |
| `SubmitUserMessageReference` | `bool` | Include user reference in submission |
| `SubmitReceiptedMessageId` | `bool` | Include receipted ID in submission |

### Status Properties
| Property | Type | Description |
|----------|------|-------------|
| `MessageState` | `MessageState?` | Final delivery state |
| `NetworkErrorCode` | `byte[]` | Network error code for failed deliveries |

## Methods

### PDU Generation
```csharp
internal IEnumerable<SendSmPDU> GetMessagePDUs(DataCoding defaultEncoding, SmppEncodingService smppEncodingService, SmppAddress destAddress, SmppAddress srcAddress)
protected abstract IEnumerable<SendSmPDU> GetPDUs(DataCoding defaultEncoding, SmppEncodingService smppEncodingService, SmppAddress destAddress = null, SmppAddress srcAddress = null)
```
**Purpose**: Converts message to SMPP PDUs for transmission
**Parameters**:
- `defaultEncoding`: Default character encoding
- `smppEncodingService`: Encoding service instance
- `destAddress`: Destination address
- `srcAddress`: Source address
**Returns**: Collection of SubmitSm PDUs

### Message Processing Flow
```mermaid
sequenceDiagram
    participant APP as Application
    participant SM as ShortMessage
    participant PDU as SendSmPDU
    participant SMSC as SMSC Server
    
    APP->>SM: Create Message
    APP->>SM: Set Properties
    APP->>SM: GetMessagePDUs()
    SM->>SM: GetPDUs()
    SM->>PDU: Create SubmitSm
    PDU->>APP: PDU Collection
    APP->>SMSC: Send PDUs
    SMSC->>APP: SubmitSmResp
    APP->>SM: Set ReceiptedMessageId
```

## Message Processing

### Single Message Processing
```mermaid
flowchart TD
    A[TextMessage] --> B{Text Length}
    B -->|<= 160| C[Single PDU]
    B -->|> 160| D[Auto-Split]
    D --> E[Generate UDH]
    E --> F[Create Multiple PDUs]
    C --> G[SubmitSm PDU]
    F --> H[SubmitSm PDUs with UDH]
```

### Multi-Part Message Processing
```mermaid
sequenceDiagram
    participant MPTM as MultiPartTextMessage
    participant UDH as UDH Generator
    participant PDU as SubmitSm PDU
    participant SEG as Segment
    
    MPTM->>UDH: Generate UDH
    UDH->>UDH: Calculate Segments
    UDH->>UDH: Assign Sequence Numbers
    UDH->>SEG: Create Segments
    SEG->>PDU: Create SubmitSm
    PDU->>MPTM: Return PDU Collection
```

### UDH (User Data Header) Structure
```mermaid
graph TB
    subgraph "UDH Structure"
        UDH[User Data Header]
        IEI[Information Element Identifier]
        IEDL[Information Element Data Length]
        IED[Information Element Data]
    end
    
    subgraph "Concatenation Info"
        REF[Reference Number]
        MAX[Maximum Number]
        SEQ[Sequence Number]
    end
    
    UDH --> IEI
    UDH --> IEDL
    UDH --> IED
    IED --> REF
    IED --> MAX
    IED --> SEQ
```

## Multi-Part Messages

### Automatic Splitting
```csharp
// TextMessage automatically splits long messages
TextMessage longMessage = new TextMessage();
longMessage.Text = "This is a very long message that will be automatically split into multiple parts when sent to the SMSC...";
longMessage.DestinationAddress = "1234567890";

// GetMessagePDUs will return multiple PDUs with UDH
var pdus = longMessage.GetMessagePDUs(DataCoding.ASCII, encodingService, destAddress, srcAddress);
// Returns multiple SubmitSm PDUs with concatenation information
```

### Manual Multi-Part Control
```csharp
// MultiPartTextMessage for explicit control
MultiPartTextMessage multiPart = new MultiPartTextMessage();
multiPart.Text = "Long message text...";
multiPart.DestinationAddress = "1234567890";
multiPart.ConcatenationType = ConcatenationType.UDH8bit; // Use UDH for concatenation

// GetMessagePDUs returns controlled segments
var pdus = multiPart.GetMessagePDUs(DataCoding.ASCII, encodingService, destAddress, srcAddress);
```

### UDH Generation
```csharp
// Use Udh for an 8-bit concatenated-message reference number.
Udh udh8 = new Udh(0x12, 3, 2);
// [0x05, 0x00, 0x03, 0x12, 0x03, 0x02]

// Use Udh16 for a 16-bit concatenated-message reference number.
// It can be passed anywhere a Udh is accepted.
Udh udh16 = new Udh16(0x1234, 3, 2);
// [0x06, 0x08, 0x04, 0x12, 0x34, 0x03, 0x02]

// TextMessage uses 8-bit references by default. Opt in to 16-bit references
// for automatic splitting and the corresponding smaller segment size.
TextMessage message = new TextMessage
{
    Text = "Long message text...",
    Use16BitReferenceNumber = true
};
```

For `Udh`, IEI `0x00` identifies an 8-bit reference and IEDL is `0x03`. For
`Udh16`, IEI `0x08` identifies a big-endian 16-bit reference and IEDL is
`0x04`. The maximum-segment and sequence-number fields remain one byte each.
The multipart limits for 16-bit UDH are 152 characters for `SMSCDefault` and
`ASCII`, 133 for `Latin1`, and 66 for `UCS2`, compared with 153, 134, and 67
respectively for 8-bit UDH.

## Usage Examples

### Basic Text Message
```csharp
// Create simple text message
TextMessage message = new TextMessage();
message.SourceAddress = "12345";
message.DestinationAddress = "1234567890";
message.Text = "Hello, World!";
message.RegisterDeliveryNotification = true;

// Send via SmppClient
client.SendMessage(message);
```

### Long Message (Auto-Split)
```csharp
// Create long message - automatically split
TextMessage longMessage = new TextMessage();
longMessage.DestinationAddress = "1234567890";
longMessage.Text = "This is a very long message that contains more than 160 characters and will be automatically split into multiple parts when sent to the SMSC server for delivery to the recipient.";
longMessage.RegisterDeliveryNotification = true;

// SmppClient handles splitting automatically
client.SendMessage(longMessage);
```

### Multi-Part Message with Control
```csharp
// Create multi-part message with explicit control
MultiPartTextMessage multiPart = new MultiPartTextMessage();
multiPart.DestinationAddress = "1234567890";
multiPart.Text = "This is a long message that will be split into multiple parts with explicit control over the splitting process and UDH generation.";
multiPart.ConcatenationType = ConcatenationType.UDH8bit;
multiPart.RegisterDeliveryNotification = true;

// Send with controlled splitting
client.SendMessage(multiPart);
```

### Message with Custom Properties
```csharp
// Create message with custom properties
TextMessage customMessage = new TextMessage();
customMessage.SourceAddress = "MyApp";
customMessage.DestinationAddress = "1234567890";
customMessage.Text = "Custom message";
customMessage.RegisterDeliveryNotification = true;
customMessage.UserMessageReference = "MSG-001";
customMessage.SubmitUserMessageReference = true;

// Send with custom reference
client.SendMessage(customMessage);
```

### Handling Delivery Receipts
```csharp
// Subscribe to delivery events
client.MessageDelivered += (sender, e) => {
    ShortMessage receipt = e.Message;
    Console.WriteLine($"Message delivered: {receipt.ReceiptedMessageId}");
    Console.WriteLine($"Message state: {receipt.MessageState}");
    Console.WriteLine($"User reference: {receipt.UserMessageReference}");
    
    if (receipt.NetworkErrorCode != null)
    {
        Console.WriteLine($"Network error: {BitConverter.ToString(receipt.NetworkErrorCode)}");
    }
};

// Send message with delivery notification
TextMessage message = new TextMessage();
message.DestinationAddress = "1234567890";
message.Text = "Message with delivery receipt";
message.RegisterDeliveryNotification = true;
message.UserMessageReference = "MSG-002";

client.SendMessage(message);
```

### Advanced PDU Generation
```csharp
// Generate PDUs manually for advanced scenarios
TextMessage message = new TextMessage();
message.DestinationAddress = "1234567890";
message.Text = "Advanced message";

// Create addresses
SmppAddress destAddress = new SmppAddress("1234567890");
SmppAddress srcAddress = new SmppAddress("12345");

// Generate PDUs
var pdus = message.GetMessagePDUs(DataCoding.ASCII, encodingService, destAddress, srcAddress);

// Send each PDU individually
foreach (var pdu in pdus)
{
    ResponsePDU response = client.SendPdu(pdu, 5000);
    if (response is SubmitSmResp submitResp)
    {
        Console.WriteLine($"Message ID: {submitResp.MessageID}");
    }
}
```

### Message State Tracking
```csharp
// Track message states
Dictionary<string, ShortMessage> sentMessages = new Dictionary<string, ShortMessage>();

// Send message and track
TextMessage message = new TextMessage();
message.UserMessageReference = Guid.NewGuid().ToString();
message.DestinationAddress = "1234567890";
message.Text = "Tracked message";
message.RegisterDeliveryNotification = true;

sentMessages[message.UserMessageReference] = message;
client.SendMessage(message);

// Handle delivery receipt
client.MessageDelivered += (sender, e) => {
    string userRef = e.Message.UserMessageReference;
    if (sentMessages.TryGetValue(userRef, out ShortMessage originalMessage))
    {
        Console.WriteLine($"Original message: {originalMessage.Text}");
        Console.WriteLine($"Delivery state: {e.Message.MessageState}");
        Console.WriteLine($"SMSC message ID: {e.Message.ReceiptedMessageId}");
    }
};
```

## Best Practices

### Message Creation
1. **Use appropriate message types**: TextMessage for simple cases, MultiPartTextMessage for control
2. **Set delivery notifications**: Enable for important messages
3. **Use meaningful references**: Set UserMessageReference for tracking
4. **Validate addresses**: Ensure phone numbers are properly formatted

### Performance Optimization
1. **Reuse message objects**: When possible, reuse instances
2. **Batch operations**: Group multiple messages when sending
3. **Monitor memory**: Watch for memory usage with large message volumes
4. **Use appropriate encoding**: Choose encoding based on content

### Error Handling
1. **Handle delivery failures**: Monitor MessageState for failures
2. **Implement retry logic**: For transient failures
3. **Log message details**: Include UserMessageReference in logs
4. **Monitor network errors**: Check NetworkErrorCode for failures

### Security Considerations
1. **Validate input**: Sanitize message content
2. **Rate limiting**: Implement appropriate sending rates
3. **Content filtering**: Filter inappropriate content
4. **Address validation**: Validate phone number formats

### Production Deployment
1. **Message queuing**: Implement queuing for high volumes
2. **Monitoring**: Track message success/failure rates
3. **Alerting**: Set up alerts for delivery failures
4. **Backup strategies**: Handle SMSC unavailability

The ShortMessage hierarchy provides a comprehensive and flexible framework for SMS message handling, supporting everything from simple text messages to complex multi-part concatenated messages with full delivery tracking capabilities.
