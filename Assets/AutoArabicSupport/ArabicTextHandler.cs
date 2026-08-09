using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using System;

[RequireComponent(typeof(TMP_Text))]
public class ArabicTextHandler : MonoBehaviour
{
    [Header("Arabic Processing Options")]
    [SerializeField]
    private bool ShowTashkeel = true;

    [SerializeField]
    private bool UseHinduNumbers = false;

    [Header("Text Spacing")]
    [SerializeField]
    private float ArabicCharacterSpacing = 0f;

    [SerializeField]
    private float UrduCharacterSpacing = 0f;

    [Header("Font Style")]
    [SerializeField]
    private FontWeight ArabicFontWeight = FontWeight.Regular;

    [SerializeField]
    private FontWeight UrduFontWeight = FontWeight.Thin;

    [Header("Error Handling")]
    [SerializeField]
    private bool SkipErrorCausingCharacters = true;

    [SerializeField]
    private bool LogErrors = true;

    [Header("Debug Info")]
    [SerializeField]
    private string LastProcessedText = "";

    [SerializeField]
    private string LastError = "";

    private TMP_Text m_textComponent;
    private bool m_isProcessing;

    private FontWeight m_defaultFontWeight;

    private void Awake()
    {
        m_textComponent = GetComponent<TMP_Text>();
        m_defaultFontWeight = m_textComponent.fontWeight;
    }

    /// <summary>
    /// Processes the localized text according to the currently selected
    /// Unity Localization language.
    ///
    /// Urdu (ur) -> UrduSupport + RTL + Urdu spacing + optional Light font
    /// Arabic (ar) -> ArabicSupport + RTL + Arabic spacing + optional Light font
    /// Everything else -> unchanged + LTR + default spacing + default font weight
    ///
    /// Assign this method to the LocalizeStringEvent "Update String" event.
    /// </summary>
    public void ProcessLocalizedText(string localizedText)
    {
        if (m_isProcessing)
            return;

        m_isProcessing = true;
        LastError = "";

        try
        {
            string processedText = ProcessText(localizedText);

            m_textComponent.text = processedText;
            LastProcessedText = processedText;
        }
        catch (Exception exception)
        {
            LastError = exception.Message;

            if (LogErrors)
            {
                Debug.LogWarning(
                    $"ArabicTextHandler: Failed to process localized text. " +
                    $"Error: {exception.Message}",
                    this);
            }

            // Fall back to the original localized string.
            m_textComponent.text = localizedText;
            LastProcessedText = localizedText;
        }
        finally
        {
            m_isProcessing = false;
        }
    }

    /// <summary>
    /// Manually processes the current TMP text using the currently selected locale.
    /// </summary>
    public void ForceProcessText()
    {
        ProcessLocalizedText(m_textComponent.text);
    }

    private string ProcessText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        if (LocalizationSettings.SelectedLocale == null)
            return text;

        string languageCode =
            LocalizationSettings.SelectedLocale.Identifier.Code;

        switch (languageCode)
        {
            case "ur":
                ConfigureUrdu();
                return SafeUrduFix(text);

            case "ar":
                ConfigureArabic();
                return SafeArabicFix(text);

            default:
                ConfigureDefault();
                return text;
        }
    }

    private void ConfigureUrdu()
    {
        m_textComponent.isRightToLeftText = true;
        m_textComponent.characterSpacing = UrduCharacterSpacing;

        m_textComponent.fontWeight = UrduFontWeight;
    }

    private void ConfigureArabic()
    {
        m_textComponent.isRightToLeftText = true;
        m_textComponent.characterSpacing = ArabicCharacterSpacing;

        m_textComponent.fontWeight = ArabicFontWeight;
    }

    private void ConfigureDefault()
    {
        m_textComponent.isRightToLeftText = false;
        m_textComponent.characterSpacing = 0f;
        m_textComponent.fontWeight = m_defaultFontWeight;
    }

    private string SafeArabicFix(string text)
    {
        try
        {
            return ArabicSupport.ArabicFixer.Fix(
                text,
                showTashkeel: ShowTashkeel,
                useHinduNumbers: UseHinduNumbers);
        }
        catch (IndexOutOfRangeException)
        {
            if (SkipErrorCausingCharacters)
                return ProcessArabicTextSafely(text);

            throw;
        }
    }

    private string SafeUrduFix(string text)
    {
        return UrduSupport.Fix(text, false);
    }

    private string ProcessArabicTextSafely(string text)
    {
        var result = new System.Text.StringBuilder(text.Length);

        for (int i = 0; i < text.Length; i++)
        {
            string currentCharacter = text[i].ToString();

            try
            {
                result.Append(
                    ArabicSupport.ArabicFixer.Fix(
                        currentCharacter,
                        showTashkeel: ShowTashkeel,
                        useHinduNumbers: UseHinduNumbers));
            }
            catch (Exception)
            {
                result.Append(currentCharacter);

                if (LogErrors)
                {
                    Debug.LogWarning(
                        $"ArabicTextHandler: Skipped problematic character " +
                        $"'{currentCharacter}' " +
                        $"(Unicode: {((int)text[i]).ToString("X4")}).",
                        this);
                }
            }
        }

        return result.ToString();
    }
}