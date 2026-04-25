using System;
using System.Collections.Generic;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Settings;

public class PechkinPaperSize
{
    private const string Size8Point5In = "8.5in";
    private const string Size9Point275In = "9.275in";
    private const string Size102Mm = "102mm";
    private const string Size105Mm = "105mm";
    private const string Size110Mm = "110mm";
    private const string Size120Mm = "120mm";
    private const string Size125Mm = "125mm";
    private const string Size148Mm = "148mm";
    private const string Size151Mm = "151mm";
    private const string Size176Mm = "176mm";
    private const string Size182Mm = "182mm";
    private const string Size210Mm = "210mm";
    private const string Size215Mm = "215mm";
    private const string Size220Mm = "220mm";
    private const string Size229Mm = "229mm";
    private const string Size230Mm = "230mm";
    private const string Size235Mm = "235mm";
    private const string Size250Mm = "250mm";
    private const string Size257Mm = "257mm";
    private const string Size297Mm = "297mm";
    private const string Size322Mm = "322mm";
    private const string Size324Mm = "324mm";
    private const string Size353Mm = "353mm";
    private const string Size420Mm = "420mm";
    private const string Size458Mm = "458mm";

    private static readonly Dictionary<PaperKind, PechkinPaperSize> Dictionary = new()
    {
        // paper sizes from http://msdn.microsoft.com/en-us/library/system.drawing.printing.paperkind.aspx
        { PaperKind.Letter, new PechkinPaperSize(Size8Point5In, "11in") },
        { PaperKind.Legal, new PechkinPaperSize(Size8Point5In, "14in") },
        { PaperKind.A4, new PechkinPaperSize(Size210Mm, Size297Mm) },
        { PaperKind.CSheet, new PechkinPaperSize("17in", "22in") },
        { PaperKind.DSheet, new PechkinPaperSize("22in", "34in") },
        { PaperKind.ESheet, new PechkinPaperSize("34in", "44in") },
        { PaperKind.LetterSmall, new PechkinPaperSize(Size8Point5In, "11in") },
        { PaperKind.Tabloid, new PechkinPaperSize("11in", "17in") },
        { PaperKind.Ledger, new PechkinPaperSize("17in", "11in") },
        { PaperKind.Statement, new PechkinPaperSize("5.5in", Size8Point5In) },
        { PaperKind.Executive, new PechkinPaperSize("7.25in", "10.5in") },
        { PaperKind.A3, new PechkinPaperSize(Size297Mm, Size420Mm) },
        { PaperKind.A4Small, new PechkinPaperSize(Size210Mm, Size297Mm) },
        { PaperKind.A5, new PechkinPaperSize(Size148Mm, Size210Mm) },
        { PaperKind.B4, new PechkinPaperSize(Size250Mm, Size353Mm) },
        { PaperKind.B5, new PechkinPaperSize(Size176Mm, Size250Mm) },
        { PaperKind.Folio, new PechkinPaperSize(Size8Point5In, "13in") },
        { PaperKind.Quarto, new PechkinPaperSize(Size215Mm, "275mm") },
        { PaperKind.Standard10x14, new PechkinPaperSize("10in", "14in") },
        { PaperKind.Standard11x17, new PechkinPaperSize("11in", "17in") },
        { PaperKind.Note, new PechkinPaperSize(Size8Point5In, "11in") },
        { PaperKind.Number9Envelope, new PechkinPaperSize("3.875in", "8.875in") },
        { PaperKind.Number10Envelope, new PechkinPaperSize("4.125in", "9.5in") },
        { PaperKind.Number11Envelope, new PechkinPaperSize("4.5in", "10.375in") },
        { PaperKind.Number12Envelope, new PechkinPaperSize("4.75in", "11in") },
        { PaperKind.Number14Envelope, new PechkinPaperSize("5in", "11.5in") },
        { PaperKind.DLEnvelope, new PechkinPaperSize(Size110Mm, Size220Mm) },
        { PaperKind.C5Envelope, new PechkinPaperSize("162mm", Size229Mm) },
        { PaperKind.C3Envelope, new PechkinPaperSize(Size324Mm, Size458Mm) },
        { PaperKind.C4Envelope, new PechkinPaperSize(Size229Mm, Size324Mm) },
        { PaperKind.C6Envelope, new PechkinPaperSize("114mm", "162mm") },
        { PaperKind.C65Envelope, new PechkinPaperSize("114mm", Size229Mm) },
        { PaperKind.B4Envelope, new PechkinPaperSize(Size250Mm, Size353Mm) },
        { PaperKind.B5Envelope, new PechkinPaperSize(Size176Mm, Size250Mm) },
        { PaperKind.B6Envelope, new PechkinPaperSize(Size176Mm, Size125Mm) },
        { PaperKind.ItalyEnvelope, new PechkinPaperSize(Size110Mm, Size230Mm) },
        { PaperKind.MonarchEnvelope, new PechkinPaperSize("3.875in", "7.5in") },
        { PaperKind.PersonalEnvelope, new PechkinPaperSize("3.625in", "6.5in") },
        { PaperKind.USStandardFanfold, new PechkinPaperSize("14.875in", "11in") },
        { PaperKind.GermanStandardFanfold, new PechkinPaperSize(Size8Point5In, "12in") },
        { PaperKind.GermanLegalFanfold, new PechkinPaperSize(Size8Point5In, "13in") },
        { PaperKind.IsoB4, new PechkinPaperSize(Size250Mm, Size353Mm) },
        { PaperKind.JapanesePostcard, new PechkinPaperSize("100mm", Size148Mm) },
        { PaperKind.Standard9x11, new PechkinPaperSize("9in", "11in") },
        { PaperKind.Standard10x11, new PechkinPaperSize("10in", "11in") },
        { PaperKind.Standard15x11, new PechkinPaperSize("15in", "11in") },
        { PaperKind.InviteEnvelope, new PechkinPaperSize(Size220Mm, Size220Mm) },
        { PaperKind.LetterExtra, new PechkinPaperSize(Size9Point275In, "12in") },
        { PaperKind.LegalExtra, new PechkinPaperSize(Size9Point275In, "15in") },
        { PaperKind.TabloidExtra, new PechkinPaperSize("11.69in", "18in") },
        { PaperKind.A4Extra, new PechkinPaperSize("236mm", Size322Mm) },
        { PaperKind.LetterTransverse, new PechkinPaperSize("8.275in", "11in") },
        { PaperKind.A4Transverse, new PechkinPaperSize(Size210Mm, Size297Mm) },
        { PaperKind.LetterExtraTransverse, new PechkinPaperSize(Size9Point275In, "12in") },
        { PaperKind.APlus, new PechkinPaperSize("227mm", "356mm") },
        { PaperKind.BPlus, new PechkinPaperSize("305mm", "487mm") },
        { PaperKind.LetterPlus, new PechkinPaperSize(Size8Point5In, "12.69in") },
        { PaperKind.A4Plus, new PechkinPaperSize(Size210Mm, "330mm") },
        { PaperKind.A5Transverse, new PechkinPaperSize(Size148Mm, Size210Mm) },
        { PaperKind.B5Transverse, new PechkinPaperSize(Size182Mm, Size257Mm) },
        { PaperKind.A3Extra, new PechkinPaperSize(Size322Mm, "445mm") },
        { PaperKind.A5Extra, new PechkinPaperSize("174mm", Size235Mm) },
        { PaperKind.B5Extra, new PechkinPaperSize("201mm", "276mm") },
        { PaperKind.A2, new PechkinPaperSize(Size420Mm, "594mm") },
        { PaperKind.A3Transverse, new PechkinPaperSize(Size297Mm, Size420Mm) },
        { PaperKind.A3ExtraTransverse, new PechkinPaperSize(Size322Mm, "445mm") },
        { PaperKind.JapaneseDoublePostcard, new PechkinPaperSize("200mm", Size148Mm) },
        { PaperKind.A6, new PechkinPaperSize(Size105Mm, Size148Mm) },
        { PaperKind.LetterRotated, new PechkinPaperSize("11in", Size8Point5In) },
        { PaperKind.A3Rotated, new PechkinPaperSize(Size420Mm, Size297Mm) },
        { PaperKind.A4Rotated, new PechkinPaperSize(Size297Mm, Size210Mm) },
        { PaperKind.A5Rotated, new PechkinPaperSize(Size210Mm, Size148Mm) },
        { PaperKind.B4JisRotated, new PechkinPaperSize("364mm", Size257Mm) },
        { PaperKind.B5JisRotated, new PechkinPaperSize(Size257Mm, Size182Mm) },
        { PaperKind.JapaneseEnvelopeChouNumber3, new PechkinPaperSize(Size120Mm, Size235Mm) },
        { PaperKind.JapaneseEnvelopeChouNumber3Rotated, new PechkinPaperSize(Size235Mm, Size120Mm) },
        { PaperKind.JapaneseEnvelopeChouNumber4, new PechkinPaperSize("90mm", "205mm") },
        { PaperKind.JapaneseEnvelopeChouNumber4Rotated, new PechkinPaperSize("205mm", "90mm") },
        { PaperKind.JapaneseEnvelopeKakuNumber2, new PechkinPaperSize("240mm", "332mm") },
        { PaperKind.JapaneseEnvelopeKakuNumber2Rotated, new PechkinPaperSize("332mm", "240mm") },
        { PaperKind.JapaneseEnvelopeKakuNumber3, new PechkinPaperSize("216mm", "277mm") },
        { PaperKind.JapaneseEnvelopeKakuNumber3Rotated, new PechkinPaperSize("277mm", "216mm") },
        { PaperKind.JapaneseEnvelopeYouNumber4, new PechkinPaperSize(Size105Mm, Size235Mm) },
        { PaperKind.JapaneseEnvelopeYouNumber4Rotated, new PechkinPaperSize(Size235Mm, Size105Mm) },
        { PaperKind.JapanesePostcardRotated, new PechkinPaperSize(Size148Mm, "100mm") },
        { PaperKind.JapaneseDoublePostcardRotated, new PechkinPaperSize(Size148Mm, "200mm") },
        { PaperKind.A6Rotated, new PechkinPaperSize(Size148Mm, Size105Mm) },
        { PaperKind.B6Jis, new PechkinPaperSize("128mm", Size182Mm) },
        { PaperKind.B6JisRotated, new PechkinPaperSize(Size182Mm, "128mm") },
        { PaperKind.Standard12x11, new PechkinPaperSize("12in", "11in") },
        { PaperKind.Prc16K, new PechkinPaperSize("146mm", Size215Mm) },
        { PaperKind.Prc32K, new PechkinPaperSize("97mm", Size151Mm) },
        { PaperKind.Prc32KBig, new PechkinPaperSize("97mm", Size151Mm) },
        { PaperKind.PrcEnvelopeNumber1, new PechkinPaperSize(Size102Mm, "165mm") },
        { PaperKind.PrcEnvelopeNumber2, new PechkinPaperSize(Size102Mm, Size176Mm) },
        { PaperKind.PrcEnvelopeNumber3, new PechkinPaperSize(Size125Mm, Size176Mm) },
        { PaperKind.PrcEnvelopeNumber4, new PechkinPaperSize(Size110Mm, "208mm") },
        { PaperKind.PrcEnvelopeNumber5, new PechkinPaperSize(Size110Mm, Size220Mm) },
        { PaperKind.PrcEnvelopeNumber6, new PechkinPaperSize(Size120Mm, Size230Mm) },
        { PaperKind.PrcEnvelopeNumber7, new PechkinPaperSize("160mm", Size230Mm) },
        { PaperKind.PrcEnvelopeNumber8, new PechkinPaperSize(Size120Mm, "309mm") },
        { PaperKind.PrcEnvelopeNumber9, new PechkinPaperSize(Size229Mm, Size324Mm) },
        { PaperKind.PrcEnvelopeNumber10, new PechkinPaperSize(Size324Mm, Size458Mm) },
        { PaperKind.Prc16KRotated, new PechkinPaperSize("146mm", Size215Mm) },
        { PaperKind.Prc32KRotated, new PechkinPaperSize("97mm", Size151Mm) },
        { PaperKind.Prc32KBigRotated, new PechkinPaperSize("97mm", Size151Mm) },
        { PaperKind.PrcEnvelopeNumber1Rotated, new PechkinPaperSize("165mm", Size102Mm) },
        { PaperKind.PrcEnvelopeNumber2Rotated, new PechkinPaperSize(Size176Mm, Size102Mm) },
        { PaperKind.PrcEnvelopeNumber3Rotated, new PechkinPaperSize(Size176Mm, Size125Mm) },
        { PaperKind.PrcEnvelopeNumber4Rotated, new PechkinPaperSize("208mm", Size110Mm) },
        { PaperKind.PrcEnvelopeNumber5Rotated, new PechkinPaperSize(Size220Mm, Size110Mm) },
        { PaperKind.PrcEnvelopeNumber6Rotated, new PechkinPaperSize(Size230Mm, Size120Mm) },
        { PaperKind.PrcEnvelopeNumber7Rotated, new PechkinPaperSize(Size230Mm, "160mm") },
        { PaperKind.PrcEnvelopeNumber8Rotated, new PechkinPaperSize("309mm", Size120Mm) },
        { PaperKind.PrcEnvelopeNumber9Rotated, new PechkinPaperSize(Size324Mm, Size229Mm) },
        { PaperKind.PrcEnvelopeNumber10Rotated, new PechkinPaperSize(Size458Mm, Size324Mm) },
    };

    public PechkinPaperSize(string width, string height)
    {
        Width = width;
        Height = height;
    }

    public string Height { get; set; }

    public string Width { get; set; }

    public static implicit operator PechkinPaperSize(PaperKind paperKind) => FromPaperKind(paperKind);

    public static PechkinPaperSize FromPaperKind(
        PaperKind paperKind)
    {
        var result = Dictionary.TryGetValue(paperKind, out PechkinPaperSize? pechkinPaperSize);
        if (!result)
        {
            throw new ArgumentOutOfRangeException(
                nameof(paperKind),
                $"Unknown paper kind {paperKind:G} - cannot convert to pechkin paper size");
        }

        if (pechkinPaperSize is null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(paperKind),
                $"Unknown paper kind {paperKind:G} - cannot convert to pechkin paper size");
        }

        return pechkinPaperSize;
    }
}
