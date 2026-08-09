// -----------------------------------------------------------------------------
// UrduSupport.cs
// Standalone Urdu text shaping helper for Unity / TextMeshPro.
//
// Usage:
//     textMesh.text = UrduSupport.Fix("جگہ سہارے");
//     textMesh.text = UrduSupport.Fix("یہ ایک اردو جملہ ہے");
//
// This class has no dependency on ArabicSupport or any other package.
// It is intended for fonts/renderers that expose Arabic Presentation Forms.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

public static class UrduSupport
{
    private const char ZWJ = '\u200D';
    private const char ZWNJ = '\u200C';

    private struct Forms
    {
        public char Isolated;
        public char Final;
        public char Initial;
        public char Medial;
        public bool JoinBefore;
        public bool JoinAfter;

        public Forms(char isolated, char final, char initial, char medial,
            bool joinBefore, bool joinAfter)
        {
            Isolated = isolated;
            Final = final;
            Initial = initial;
            Medial = medial;
            JoinBefore = joinBefore;
            JoinAfter = joinAfter;
        }
    }

    // Urdu/Arabic Unicode characters -> Arabic Presentation Forms.
    // Characters with no presentation form are intentionally left untouched.
    private static readonly Dictionary<char, Forms> FormsMap = new Dictionary<char, Forms>
    {
        // Arabic letters commonly used by Urdu.
        ['ء'] = new Forms('\uFE80', '\uFE80', '\uFE80', '\uFE80', false, false),
        ['آ'] = new Forms('\uFE81', '\uFE82', '\uFE82', '\uFE82', false, false),
        ['أ'] = new Forms('\uFE83', '\uFE84', '\uFE84', '\uFE84', false, false),
        ['ؤ'] = new Forms('\uFE85', '\uFE86', '\uFE86', '\uFE86', false, false),
        ['إ'] = new Forms('\uFE87', '\uFE88', '\uFE88', '\uFE88', false, false),
        ['ئ'] = new Forms('\uFE89', '\uFE8A', '\uFE8B', '\uFE8C', true, true),
        ['ا'] = new Forms('\uFE8D', '\uFE8E', '\uFE8E', '\uFE8E', false, false),
        ['ب'] = new Forms('\uFE8F', '\uFE90', '\uFE91', '\uFE92', true, true),
        ['ة'] = new Forms('\uFE93', '\uFE94', '\uFE94', '\uFE94', false, false),
        ['ت'] = new Forms('\uFE95', '\uFE96', '\uFE97', '\uFE98', true, true),
        ['ث'] = new Forms('\uFE99', '\uFE9A', '\uFE9B', '\uFE9C', true, true),
        ['ج'] = new Forms('\uFE9D', '\uFE9E', '\uFE9F', '\uFEA0', true, true),
        ['ح'] = new Forms('\uFEA1', '\uFEA2', '\uFEA3', '\uFEA4', true, true),
        ['خ'] = new Forms('\uFEA5', '\uFEA6', '\uFEA7', '\uFEA8', true, true),
        ['د'] = new Forms('\uFEA9', '\uFEAA', '\uFEAA', '\uFEAA', false, false),
        ['ذ'] = new Forms('\uFEAB', '\uFEAC', '\uFEAC', '\uFEAC', false, false),
        ['ر'] = new Forms('\uFEAD', '\uFEAE', '\uFEAE', '\uFEAE', false, false),
        ['ز'] = new Forms('\uFEAF', '\uFEB0', '\uFEB0', '\uFEB0', false, false),
        ['س'] = new Forms('\uFEB1', '\uFEB2', '\uFEB3', '\uFEB4', true, true),
        ['ش'] = new Forms('\uFEB5', '\uFEB6', '\uFEB7', '\uFEB8', true, true),
        ['ص'] = new Forms('\uFEB9', '\uFEBA', '\uFEBB', '\uFEBC', true, true),
        ['ض'] = new Forms('\uFEBD', '\uFEBE', '\uFEBF', '\uFEC0', true, true),
        ['ط'] = new Forms('\uFEC1', '\uFEC2', '\uFEC3', '\uFEC4', true, true),
        ['ظ'] = new Forms('\uFEC5', '\uFEC6', '\uFEC7', '\uFEC8', true, true),
        ['ع'] = new Forms('\uFEC9', '\uFECA', '\uFECB', '\uFECC', true, true),
        ['غ'] = new Forms('\uFECD', '\uFECE', '\uFECF', '\uFED0', true, true),
        ['ف'] = new Forms('\uFED1', '\uFED2', '\uFED3', '\uFED4', true, true),
        ['ق'] = new Forms('\uFED5', '\uFED6', '\uFED7', '\uFED8', true, true),
        ['ك'] = new Forms('\uFED9', '\uFEDA', '\uFEDB', '\uFEDC', true, true),
        ['ل'] = new Forms('\uFEDD', '\uFEDE', '\uFEDF', '\uFEE0', true, true),
        ['م'] = new Forms('\uFEE1', '\uFEE2', '\uFEE3', '\uFEE4', true, true),
        ['ن'] = new Forms('\uFEE5', '\uFEE6', '\uFEE7', '\uFEE8', true, true),
        ['ه'] = new Forms('\uFEE9', '\uFEEA', '\uFEEB', '\uFEEC', true, true),
        ['و'] = new Forms('\uFEED', '\uFEEE', '\uFEEE', '\uFEEE', false, false),
        ['ى'] = new Forms('\uFEEF', '\uFEF0', '\uFEF0', '\uFEF0', false, false),
        ['ي'] = new Forms('\uFEF1', '\uFEF2', '\uFEF3', '\uFEF4', true, true),

        // Urdu/Persian additions.
        ['پ'] = new Forms('\uFB56', '\uFB57', '\uFB58', '\uFB59', true, true),
        ['چ'] = new Forms('\uFB7A', '\uFB7B', '\uFB7C', '\uFB7D', true, true),
        ['ژ'] = new Forms('\uFB8A', '\uFB8B', '\uFB8B', '\uFB8B', false, false),
        ['گ'] = new Forms('\uFB92', '\uFB93', '\uFB94', '\uFB95', true, true),
        ['ک'] = new Forms('\uFB8E', '\uFB8F', '\uFB90', '\uFB91', true, true),
        ['ی'] = new Forms('\uFBFC', '\uFBFD', '\uFBFE', '\uFBFF', true, true),

        // Urdu-specific retroflex and aspirated letters.
        ['ٹ'] = new Forms('\uFB66', '\uFB67', '\uFB68', '\uFB69', true, true),
        ['ڈ'] = new Forms('\uFB88', '\uFB89', '\uFB89', '\uFB89', false, false),
        ['ڑ'] = new Forms('\uFB5A', '\uFB5B', '\uFB5B', '\uFB5B', false, false),

        // Urdu heh variants.
        ['ھ'] = new Forms('\uFBA6', '\uFBA7', '\uFBA8', '\uFBA9', true, true),
        ['ہ'] = new Forms('\uFBAA', '\uFBAB', '\uFBAC', '\uFBAD', true, true),
        ['ۂ'] = new Forms('\uFBAE', '\uFBAF', '\uFBB0', '\uFBB1', true, true),

        // Urdu noon ghunna.
        ['ں'] = new Forms('\uFBE8', '\uFBE9', '\uFBE9', '\uFBE9', false, false),

        // Urdu bari ye.
        ['ے'] = new Forms('\uFBAE', '\uFBAF', '\uFBB0', '\uFBB1', false, false),
    };

    private static readonly HashSet<char> CombiningMarks = new HashSet<char>
    {
        '\u0610','\u0611','\u0612','\u0613','\u0614',
        '\u0615','\u0616','\u0617','\u0618','\u0619',
        '\u061A','\u064B','\u064C','\u064D','\u064E',
        '\u064F','\u0650','\u0651','\u0652','\u0653',
        '\u0654','\u0655','\u0656','\u0657','\u0658',
        '\u0659','\u065A','\u065B','\u065C','\u065D',
        '\u065E','\u065F','\u0670','\u06D6','\u06D7',
        '\u06D8','\u06D9','\u06DA','\u06DB','\u06DC',
        '\u06DF','\u06E0','\u06E1','\u06E2','\u06E3',
        '\u06E4','\u06E7','\u06E8','\u06EA','\u06EB',
        '\u06EC','\u06ED'
    };

    /// <summary>
    /// Shapes Urdu text and reverses RTL runs for renderers that do not perform
    /// Arabic/Urdu shaping and bidirectional layout themselves.
    /// </summary>
    public static string Fix(string text)
    {
        return Fix(text, true);
    }

    /// <summary>
    /// Shapes Urdu text.
    /// </summary>
    /// <param name="text">Input Unicode Urdu text.</param>
    /// <param name="reverseRtl">Reverse Urdu runs for a left-to-right renderer.</param>
    public static string Fix(string text, bool reverseRtl)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var result = new StringBuilder(text.Length * 2);
        int lineStart = 0;

        while (lineStart < text.Length)
        {
            int lineEnd = text.IndexOf('\n', lineStart);
            if (lineEnd < 0)
                lineEnd = text.Length;

            result.Append(FixLine(text, lineStart, lineEnd, reverseRtl));

            if (lineEnd < text.Length)
                result.Append('\n');

            lineStart = lineEnd + 1;
        }

        return result.ToString();
    }

    private static string FixLine(string text, int start, int end, bool reverseRtl)
    {
        var output = new StringBuilder(end - start);

        int i = start;

        while (i < end)
        {
            if (IsUrduCharacter(text[i]))
            {
                int runEnd = i + 1;

                while (runEnd < end && IsRtlRunCharacter(text[runEnd]))
                    runEnd++;

                string shaped = ShapeRun(text, i, runEnd);

                if (reverseRtl)
                    ReverseRunPreservingCombiningMarks(shaped, output);
                else
                    output.Append(shaped);

                i = runEnd;
            }
            else
            {
                output.Append(text[i]);
                i++;
            }
        }

        return output.ToString();
    }

    private static string ShapeRun(string text, int start, int end)
    {
        var chars = new List<char>(end - start);

        for (int i = start; i < end; i++)
        {
            char c = text[i];

            // Keep ZWNJ as a joining boundary, but do not render it.
            if (c == ZWNJ)
            {
                chars.Add(ZWNJ);
                continue;
            }

            // ZWJ is intentionally retained so fonts can use it where needed.
            if (c == ZWJ)
            {
                chars.Add(ZWJ);
                continue;
            }

            chars.Add(c);
        }

        var output = new StringBuilder(chars.Count);

        for (int i = 0; i < chars.Count; i++)
        {
            char current = chars[i];

            if (!FormsMap.TryGetValue(current, out Forms currentForms))
            {
                output.Append(current);
                continue;
            }

            int previous = FindPreviousJoinable(chars, i);
            int next = FindNextJoinable(chars, i);

            bool joinsPrevious = previous >= 0 &&
                                 CanJoinAfter(chars[previous]) &&
                                 currentForms.JoinBefore &&
                                 !HasJoinBoundary(chars, previous, i);

            bool joinsNext = next >= 0 &&
                             currentForms.JoinAfter &&
                             CanJoinBefore(chars[next]) &&
                             !HasJoinBoundary(chars, i, next);

            if (joinsPrevious && joinsNext)
                output.Append(currentForms.Medial);
            else if (joinsPrevious)
                output.Append(currentForms.Final);
            else if (joinsNext)
                output.Append(currentForms.Initial);
            else
                output.Append(currentForms.Isolated);
        }

        return output.ToString();
    }

    private static int FindPreviousJoinable(List<char> chars, int index)
    {
        int i = index - 1;

        while (i >= 0 && CombiningMarks.Contains(chars[i]))
            i--;

        return i;
    }

    private static int FindNextJoinable(List<char> chars, int index)
    {
        int i = index + 1;

        while (i < chars.Count && CombiningMarks.Contains(chars[i]))
            i++;

        return i < chars.Count ? i : -1;
    }

    private static bool HasJoinBoundary(List<char> chars, int from, int to)
    {
        for (int i = from + 1; i < to; i++)
        {
            if (chars[i] == ZWNJ)
                return true;
        }

        return false;
    }

    private static bool CanJoinAfter(char c)
    {
        if (!FormsMap.TryGetValue(c, out Forms forms))
            return false;

        return forms.JoinAfter;
    }

    private static bool CanJoinBefore(char c)
    {
        if (!FormsMap.TryGetValue(c, out Forms forms))
            return false;

        return forms.JoinBefore;
    }

    private static bool IsUrduCharacter(char c)
    {
        return FormsMap.ContainsKey(c);
    }

    private static bool IsRtlRunCharacter(char c)
    {
        return IsUrduCharacter(c) ||
               CombiningMarks.Contains(c) ||
               c == ZWJ ||
               c == ZWNJ;
    }

    private static void ReverseRunPreservingCombiningMarks(
        string shaped,
        StringBuilder output)
    {
        int i = shaped.Length - 1;

        while (i >= 0)
        {
            int markStart = i;

            while (markStart >= 0 && CombiningMarks.Contains(shaped[markStart]))
                markStart--;

            if (markStart < i)
            {
                // Combining marks belong to the preceding base character.
                if (markStart >= 0)
                {
                    output.Append(shaped[markStart]);

                    for (int j = markStart + 1; j <= i; j++)
                        output.Append(shaped[j]);

                    i = markStart - 1;
                }
                else
                {
                    for (int j = i; j >= 0; j--)
                        output.Append(shaped[j]);

                    break;
                }
            }
            else
            {
                output.Append(shaped[i]);
                i--;
            }
        }
    }

    /// <summary>
    /// Returns true when the supplied character is handled by UrduSupport.
    /// </summary>
    public static bool IsUrdu(char character)
    {
        return IsUrduCharacter(character);
    }

    /// <summary>
    /// Returns true for Unicode Urdu/Arabic combining marks handled by this class.
    /// </summary>
    public static bool IsCombiningMark(char character)
    {
        return CombiningMarks.Contains(character);
    }
}