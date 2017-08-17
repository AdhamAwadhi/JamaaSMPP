# JamaaSMPP
Jamaa SMPP Client is a .NET implementation of the SMPP protocol that focuses on providing an easy-to-use and robust SMPP client library for .NET developers. This project is intended to be used by developers who want to integrate SMS functionalities in their applications as well as students who are learning the SMPP protocol.

## Based On
This is created based on  https://jamaasmpp.codeplex.com/

### [Wiki](https://github.com/AdhamAwadhi/JamaaSMPP/wiki)

## [NuGet](https://www.nuget.org/packages/JamaaSMPP) ![#](https://img.shields.io/nuget/v/JamaaSMPP.svg)
	Install-Package JamaaSMPP


## SMPP Server Simulator
- Downlad from [http://www.seleniumsoftware.com/downloads.html](http://www.seleniumsoftware.com/downloads.html)
- Read User Guide [http://www.seleniumsoftware.com/user-guide.htm](http://www.seleniumsoftware.com/user-guide.htm)

# What's new?
- Support sending concatenated messages
- Fix Unicode Languages [issue #2](https://github.com/AdhamAwadhi/JamaaSMPP/issues/2). Allow user to set UCS2 encoding in SMPPEncodingUtil
 
        SMPPEncodingUtil.UCS2Encoding = Encoding.BigEndianUnicode; 

- Add `TextMessage` virtual method `CreateSubmitSm()` to allow user to change some properties (like Source address ton)

        class MyTextMessage : TextMessage
        {
            protected override SubmitSm CreateSubmitSm()
            {
                var sm = base.CreateSubmitSm();
                sm.SourceAddress.Ton = JamaaTech.Smpp.Net.Lib.TypeOfNumber.Aphanumeric;
                return sm;
            }
        }

- Add Message `UserMessageReference`, you can now use it on `MessageSent` event 

        // set user message reference
        msg.UserMessageReference = Guid.NewGuid().ToString();

    on MessageSent event

        Console.WriteLine("Message Id {0} Sent to: {1}", e.ShortMessage.UserMessageReference, e.ShortMessage.DestinationAddress);

- Fix `SMSCDefaultEncoding` exception [issue #4](https://github.com/AdhamAwadhi/JamaaSMPP/issues/4). There are 2 options here:
    - GSM Encoding (***Default***) : Use GSM 03.38 alphabet [Wikipedia](https://en.wikipedia.org/wiki/GSM_03.38). (Code from [https://github.com/mediaburst/.NET-GSM-Encoding/blob/master/GSMEncoding.cs](https://github.com/mediaburst/.NET-GSM-Encoding/blob/master/GSMEncoding.cs))
    - JamaaSMPP version : Simple version of GSM 03.38 without **Greeks  alphapet**

  >    You can choose between them by setting `UseGsmEncoding` property to `true` (*defualt*) to use  **GSM Encoding** or ti `false` to use the other.
    
        JamaaTech.Smpp.Net.Lib.Util.SMSCDefaultEncoding.UseGsmEncoding = true; // or false