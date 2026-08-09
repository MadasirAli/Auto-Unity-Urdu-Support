using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Urdu-only contextual text shaper.
///
/// IMPORTANT:
/// This class does NOT perform RTL ordering.
///
/// TextMeshPro should be configured with:
///
///     text.isRightToLeftText = true;
///
/// Usage:
///
///     string shaped = UrduSupport.Fix("ساخت میں تبدیلی");
///
/// Expected:
///
///     ﺳﺎﺧﺖ ﻣﯿﮟ ﺗﺒﺪﯾﻠﯽ
///
/// The returned string remains in logical order.
/// TMP is responsible for displaying it RTL.
/// </summary>
public static class UrduSupport
{
    private const char ZWJ = '\u200D';
    private const char ZWNJ = '\u200C';

    private enum JoinType
    {
        None,

        // Can connect to the character before it,
        // but cannot connect to the character after it.
        Right,

        // Can connect both before and after.
        Dual
    }

    private readonly struct UrduLetter
    {
        public readonly char Isolated;
        public readonly char Final;
        public readonly char Initial;
        public readonly char Medial;
        public readonly JoinType JoinType;

        public UrduLetter(
            char isolated,
            char final,
            char initial,
            char medial,
            JoinType joinType)
        {
            Isolated = isolated;
            Final = final;
            Initial = initial;
            Medial = medial;
            JoinType = joinType;
        }
    }

    // =========================================================================
    // Urdu alphabet
    // =========================================================================

    private static readonly Dictionary<char, UrduLetter> Letters =
        new Dictionary<char, UrduLetter>
        {
            // -----------------------------------------------------------------
            // Hamza / Alif family
            // -----------------------------------------------------------------

            ['ء'] = new UrduLetter(
                '\uFE80',
                '\uFE80',
                '\uFE80',
                '\uFE80',
                JoinType.None),

            ['آ'] = new UrduLetter(
                '\uFE81',
                '\uFE82',
                '\uFE82',
                '\uFE82',
                JoinType.Right),

            ['أ'] = new UrduLetter(
                '\uFE83',
                '\uFE84',
                '\uFE84',
                '\uFE84',
                JoinType.Right),

            ['ؤ'] = new UrduLetter(
                '\uFE85',
                '\uFE86',
                '\uFE86',
                '\uFE86',
                JoinType.Right),

            ['إ'] = new UrduLetter(
                '\uFE87',
                '\uFE88',
                '\uFE88',
                '\uFE88',
                JoinType.Right),

            ['ا'] = new UrduLetter(
                '\uFE8D',
                '\uFE8E',
                '\uFE8E',
                '\uFE8E',
                JoinType.Right),

            // -----------------------------------------------------------------
            // ب پ ت ٹ ث
            // -----------------------------------------------------------------

            ['ب'] = new UrduLetter(
                '\uFE8F',
                '\uFE90',
                '\uFE91',
                '\uFE92',
                JoinType.Dual),

            ['پ'] = new UrduLetter(
                '\uFB56',
                '\uFB57',
                '\uFB58',
                '\uFB59',
                JoinType.Dual),

            ['ت'] = new UrduLetter(
                '\uFE95',
                '\uFE96',
                '\uFE97',
                '\uFE98',
                JoinType.Dual),

            ['ٹ'] = new UrduLetter(
                '\uFB66',
                '\uFB67',
                '\uFB68',
                '\uFB69',
                JoinType.Dual),

            ['ث'] = new UrduLetter(
                '\uFE99',
                '\uFE9A',
                '\uFE9B',
                '\uFE9C',
                JoinType.Dual),

            // -----------------------------------------------------------------
            // ج چ ح خ
            // -----------------------------------------------------------------

            ['ج'] = new UrduLetter(
                '\uFE9D',
                '\uFE9E',
                '\uFE9F',
                '\uFEA0',
                JoinType.Dual),

            ['چ'] = new UrduLetter(
                '\uFB7A',
                '\uFB7B',
                '\uFB7C',
                '\uFB7D',
                JoinType.Dual),

            ['ح'] = new UrduLetter(
                '\uFEA1',
                '\uFEA2',
                '\uFEA3',
                '\uFEA4',
                JoinType.Dual),

            ['خ'] = new UrduLetter(
                '\uFEA5',
                '\uFEA6',
                '\uFEA7',
                '\uFEA8',
                JoinType.Dual),

            // -----------------------------------------------------------------
            // د ڈ ذ ر ڑ ز ژ
            // -----------------------------------------------------------------

            ['د'] = new UrduLetter(
                '\uFEA9',
                '\uFEAA',
                '\uFEAA',
                '\uFEAA',
                JoinType.Right),

            ['ڈ'] = new UrduLetter(
                '\uFB88',
                '\uFB89',
                '\uFB88',
                '\uFB89',
                JoinType.Right),

            ['ذ'] = new UrduLetter(
                '\uFEAB',
                '\uFEAC',
                '\uFEAC',
                '\uFEAC',
                JoinType.Right),

            ['ر'] = new UrduLetter(
                '\uFEAD',
                '\uFEAE',
                '\uFEAE',
                '\uFEAE',
                JoinType.Right),

            ['ڑ'] = new UrduLetter(
                '\uFB5A',
                '\uFB5B',
                '\uFB5A',
                '\uFB5B',
                JoinType.Right),

            ['ز'] = new UrduLetter(
                '\uFEAF',
                '\uFEB0',
                '\uFEB0',
                '\uFEB0',
                JoinType.Right),

            ['ژ'] = new UrduLetter(
                '\uFB8A',
                '\uFB8B',
                '\uFB8A',
                '\uFB8B',
                JoinType.Right),

            // -----------------------------------------------------------------
            // س ش ص ض ط ظ
            // -----------------------------------------------------------------

            ['س'] = new UrduLetter(
                '\uFEB1',
                '\uFEB2',
                '\uFEB3',
                '\uFEB4',
                JoinType.Dual),

            ['ش'] = new UrduLetter(
                '\uFEB5',
                '\uFEB6',
                '\uFEB7',
                '\uFEB8',
                JoinType.Dual),

            ['ص'] = new UrduLetter(
                '\uFEB9',
                '\uFEBA',
                '\uFEBB',
                '\uFEBC',
                JoinType.Dual),

            ['ض'] = new UrduLetter(
                '\uFEBD',
                '\uFEBE',
                '\uFEBF',
                '\uFEC0',
                JoinType.Dual),

            ['ط'] = new UrduLetter(
                '\uFEC1',
                '\uFEC2',
                '\uFEC3',
                '\uFEC4',
                JoinType.Dual),

            ['ظ'] = new UrduLetter(
                '\uFEC5',
                '\uFEC6',
                '\uFEC7',
                '\uFEC8',
                JoinType.Dual),

            // -----------------------------------------------------------------
            // ع غ ف ق ک گ
            // -----------------------------------------------------------------

            ['ع'] = new UrduLetter(
                '\uFEC9',
                '\uFECA',
                '\uFECB',
                '\uFECC',
                JoinType.Dual),

            ['غ'] = new UrduLetter(
                '\uFECD',
                '\uFECE',
                '\uFECF',
                '\uFED0',
                JoinType.Dual),

            ['ف'] = new UrduLetter(
                '\uFED1',
                '\uFED2',
                '\uFED3',
                '\uFED4',
                JoinType.Dual),

            ['ق'] = new UrduLetter(
                '\uFED5',
                '\uFED6',
                '\uFED7',
                '\uFED8',
                JoinType.Dual),

            // Urdu Kaf: ک
            ['ک'] = new UrduLetter(
                '\uFB8E',
                '\uFB8F',
                '\uFB90',
                '\uFB91',
                JoinType.Dual),

            ['گ'] = new UrduLetter(
                '\uFB92',
                '\uFB93',
                '\uFB94',
                '\uFB95',
                JoinType.Dual),

            // Arabic Kaf may occur in Urdu text as well.
            ['ك'] = new UrduLetter(
                '\uFED9',
                '\uFEDA',
                '\uFEDB',
                '\uFEDC',
                JoinType.Dual),

            // -----------------------------------------------------------------
            // ل م ن
            // -----------------------------------------------------------------

            ['ل'] = new UrduLetter(
                '\uFEDD',
                '\uFEDE',
                '\uFEDF',
                '\uFEE0',
                JoinType.Dual),

            ['م'] = new UrduLetter(
                '\uFEE1',
                '\uFEE2',
                '\uFEE3',
                '\uFEE4',
                JoinType.Dual),

            ['ن'] = new UrduLetter(
                '\uFEE5',
                '\uFEE6',
                '\uFEE7',
                '\uFEE8',
                JoinType.Dual),

            // -----------------------------------------------------------------
            // ں
            //
            // Noon ghunna is a right-joining letter:
            //
            //     م + ی + ں
            //
            // becomes:
            //
            //     ﻣ + ﯿ + ﮟ
            // -----------------------------------------------------------------

            ['ں'] = new UrduLetter(
                '\uFB9E',
                '\uFB9F',
                '\uFB9E',
                '\uFB9F',
                JoinType.Right),

            // -----------------------------------------------------------------
            // ہ ھ
            // -----------------------------------------------------------------

            ['ہ'] = new UrduLetter(
                '\uFBA6',
                '\uFBA7',
                '\uFBA8',
                '\uFBA9',
                JoinType.Dual),

            ['ھ'] = new UrduLetter(
                '\uFBAA',
                '\uFBAB',
                '\uFBAC',
                '\uFBAD',
                JoinType.Dual),

            ['ۂ'] = new UrduLetter(
                '\uFBAE',
                '\uFBAF',
                '\uFBB0',
                '\uFBB1',
                JoinType.Dual),

            // -----------------------------------------------------------------
            // و
            // -----------------------------------------------------------------

            ['و'] = new UrduLetter(
                '\uFEED',
                '\uFEEE',
                '\uFEEE',
                '\uFEEE',
                JoinType.Right),

            // -----------------------------------------------------------------
            // ی
            //
            // Urdu Yeh U+06CC:
            //
            // isolated = FBFC  ﯼ
            // final    = FBFD  ﯽ
            // initial  = FBFE  ﯾ
            // medial   = FBFF  ﯿ
            //
            // This is particularly important for:
            //
            //     میں
            //
            // which must produce:
            //
            //     ﻣﯿﮟ
            // -----------------------------------------------------------------

            ['ی'] = new UrduLetter(
                '\uFBFC',
                '\uFBFD',
                '\uFBFE',
                '\uFBFF',
                JoinType.Dual),

            // Arabic Yeh
            ['ي'] = new UrduLetter(
                '\uFEF1',
                '\uFEF2',
                '\uFEF3',
                '\uFEF4',
                JoinType.Dual),

            // -----------------------------------------------------------------
            // ے
            // -----------------------------------------------------------------

            ['ے'] = new UrduLetter(
                '\uFBAE',
                '\uFBAF',
                '\uFBAE',
                '\uFBAF',
                JoinType.Right),

            // -----------------------------------------------------------------
            // ى
            // -----------------------------------------------------------------

            ['ى'] = new UrduLetter(
                '\uFEEF',
                '\uFEF0',
                '\uFEF0',
                '\uFEF0',
                JoinType.Right),

            // -----------------------------------------------------------------
            // ة
            // -----------------------------------------------------------------

            ['ة'] = new UrduLetter(
                '\uFE93',
                '\uFE94',
                '\uFE94',
                '\uFE94',
                JoinType.Right),

            // -----------------------------------------------------------------
            // ئ
            // -----------------------------------------------------------------

            ['ئ'] = new UrduLetter(
                '\uFE89',
                '\uFE8A',
                '\uFE8B',
                '\uFE8C',
                JoinType.Dual),
        };

    // =========================================================================
    // Combining marks
    // =========================================================================

    private static readonly HashSet<char> CombiningMarks =
        new HashSet<char>
        {
            '\u0610',
            '\u0611',
            '\u0612',
            '\u0613',
            '\u0614',
            '\u0615',
            '\u0616',
            '\u0617',
            '\u0618',
            '\u0619',
            '\u061A',

            '\u064B',
            '\u064C',
            '\u064D',
            '\u064E',
            '\u064F',
            '\u0650',
            '\u0651',
            '\u0652',
            '\u0653',
            '\u0654',
            '\u0655',
            '\u0656',
            '\u0657',
            '\u0658',
            '\u0659',
            '\u065A',
            '\u065B',
            '\u065C',
            '\u065D',
            '\u065E',
            '\u065F',

            '\u0670',

            '\u06D6',
            '\u06D7',
            '\u06D8',
            '\u06D9',
            '\u06DA',
            '\u06DB',
            '\u06DC',
            '\u06DF',
            '\u06E0',
            '\u06E1',
            '\u06E2',
            '\u06E3',
            '\u06E4',
            '\u06E7',
            '\u06E8',
            '\u06EA',
            '\u06EB',
            '\u06EC',
            '\u06ED'
        };

    // =========================================================================
    // Public API
    // =========================================================================

    /// <summary>
    /// Urdu contextual shaping only.
    ///
    /// No RTL reversal.
    /// </summary>
    public static string Fix(string text)
    {
        return Fix(text, false);
    }

    /// <summary>
    /// Urdu contextual shaping.
    ///
    /// reverseRtl is retained only for compatibility with the previous API.
    /// For TextMeshPro use false.
    /// </summary>
    public static string Fix(
        string text,
        bool reverseRtl)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        StringBuilder result =
            new StringBuilder(text.Length * 2);

        int lineStart = 0;

        while (lineStart < text.Length)
        {
            int lineEnd =
                text.IndexOf('\n', lineStart);

            if (lineEnd < 0)
                lineEnd = text.Length;

            ShapeLine(
                text,
                lineStart,
                lineEnd,
                result);

            if (lineEnd < text.Length)
                result.Append('\n');

            lineStart = lineEnd + 1;
        }

        // reverseRtl intentionally ignored.
        //
        // TMP handles RTL ordering.
        return result.ToString();
    }

    // =========================================================================
    // Line
    // =========================================================================

    private static void ShapeLine(
        string text,
        int start,
        int end,
        StringBuilder result)
    {
        int index = start;

        while (index < end)
        {
            if (!IsUrduLetter(text[index]))
            {
                result.Append(text[index]);
                index++;
                continue;
            }

            int runEnd = index + 1;

            while (runEnd < end &&
                   IsUrduRunCharacter(text[runEnd]))
            {
                runEnd++;
            }

            ShapeRun(
                text,
                index,
                runEnd,
                result);

            index = runEnd;
        }
    }

    // =========================================================================
    // Run shaping
    // =========================================================================

    private static void ShapeRun(
        string text,
        int start,
        int end,
        StringBuilder result)
    {
        for (int i = start; i < end; i++)
        {
            char current = text[i];

            // -------------------------------------------------------------
            // Combining marks remain untouched.
            // -------------------------------------------------------------

            if (CombiningMarks.Contains(current))
            {
                result.Append(current);
                continue;
            }

            // -------------------------------------------------------------
            // ZWJ/ZWNJ are control characters, not glyphs.
            // -------------------------------------------------------------

            if (current == ZWJ ||
                current == ZWNJ)
            {
                continue;
            }

            if (!Letters.TryGetValue(
                    current,
                    out UrduLetter forms))
            {
                result.Append(current);
                continue;
            }

            int previous =
                FindPreviousLetter(
                    text,
                    start,
                    i);

            int next =
                FindNextLetter(
                    text,
                    i,
                    end);

            bool joinsPrevious =
                previous >= 0 &&
                !HasZWNJ(
                    text,
                    previous,
                    i) &&
                CanJoinForward(
                    text[previous]) &&
                CanJoinBackward(
                    current);

            bool joinsNext =
                next >= 0 &&
                !HasZWNJ(
                    text,
                    i,
                    next) &&
                CanJoinForward(
                    current) &&
                CanJoinBackward(
                    text[next]);

            // -------------------------------------------------------------
            // Contextual form selection.
            // -------------------------------------------------------------

            if (joinsPrevious && joinsNext)
            {
                result.Append(forms.Medial);
            }
            else if (joinsPrevious)
            {
                result.Append(forms.Final);
            }
            else if (joinsNext)
            {
                result.Append(forms.Initial);
            }
            else
            {
                result.Append(forms.Isolated);
            }
        }
    }

    // =========================================================================
    // Joining logic
    // =========================================================================

    private static bool CanJoinForward(
        char character)
    {
        if (!Letters.TryGetValue(
                character,
                out UrduLetter letter))
        {
            return false;
        }

        return letter.JoinType ==
                   JoinType.Dual;
    }

    private static bool CanJoinBackward(
        char character)
    {
        if (!Letters.TryGetValue(
                character,
                out UrduLetter letter))
        {
            return false;
        }

        return letter.JoinType ==
                   JoinType.Right ||
               letter.JoinType ==
                   JoinType.Dual;
    }

    private static int FindPreviousLetter(
        string text,
        int start,
        int index)
    {
        for (int i = index - 1;
             i >= start;
             i--)
        {
            char c = text[i];

            if (CombiningMarks.Contains(c))
                continue;

            if (c == ZWJ)
                continue;

            if (c == ZWNJ)
                return -1;

            if (Letters.ContainsKey(c))
                return i;

            return -1;
        }

        return -1;
    }

    private static int FindNextLetter(
        string text,
        int index,
        int end)
    {
        for (int i = index + 1;
             i < end;
             i++)
        {
            char c = text[i];

            if (CombiningMarks.Contains(c))
                continue;

            if (c == ZWJ)
                continue;

            if (c == ZWNJ)
                return -1;

            if (Letters.ContainsKey(c))
                return i;

            return -1;
        }

        return -1;
    }

    private static bool HasZWNJ(
        string text,
        int from,
        int to)
    {
        for (int i = from + 1;
             i < to;
             i++)
        {
            if (text[i] == ZWNJ)
                return true;
        }

        return false;
    }

    // =========================================================================
    // Classification
    // =========================================================================

    private static bool IsUrduLetter(
        char character)
    {
        return Letters.ContainsKey(character);
    }

    private static bool IsUrduRunCharacter(
        char character)
    {
        return Letters.ContainsKey(character) ||
               CombiningMarks.Contains(character) ||
               character == ZWJ ||
               character == ZWNJ;
    }

    // =========================================================================
    // Public helpers
    // =========================================================================

    public static bool IsUrdu(
        char character)
    {
        return Letters.ContainsKey(character);
    }

    public static bool IsCombiningMark(
        char character)
    {
        return CombiningMarks.Contains(character);
    }
}