using GeneralSurvey.Api.Models;

namespace GeneralSurvey.Api.Services;

public class SurveyParser
{
    public List<Survey> Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var surveys = new List<Survey>();
        var lines = content.Split(
            ['\r', '\n'],
            StringSplitOptions.RemoveEmptyEntries);

        Survey? currentSurvey = null;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (line.StartsWith("Sondage ", StringComparison.OrdinalIgnoreCase))
            {
                currentSurvey = ParseSurvey(line);
                surveys.Add(currentSurvey);
                continue;
            }

            if (currentSurvey is null)
            {
                throw new FormatException(
                    "Une question ne peut pas être définie avant un sondage.");
            }

            currentSurvey.Questions.Add(ParseQuestion(line));
        }

        return surveys;
    }

    private static Survey ParseSurvey(string line)
    {
        const string prefix = "Sondage ";

        var title = line[prefix.Length..].Trim();
        title = title.TrimEnd(':').Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new FormatException("Le sondage doit avoir un numéro.");
        }

        if (!int.TryParse(title, out var id))
        {
            throw new FormatException(
                $"Le numéro du sondage est invalide : {title}");
        }

        return new Survey
        {
            Id = id,
            Title = $"Sondage {id}"
        };
    }

    private static Question ParseQuestion(string line)
    {
        var separatorIndex = line.IndexOf('.');

        if (separatorIndex <= 0)
        {
            throw new FormatException(
                $"Format de question invalide : {line}");
        }

        var questionIdText = line[..separatorIndex].Trim();

        if (!int.TryParse(questionIdText, out var questionId))
        {
            throw new FormatException(
                $"Le numéro de question est invalide : {questionIdText}");
        }

        var content = line[(separatorIndex + 1)..].Trim();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new FormatException("La question ne peut pas être vide.");
        }

        var optionSeparatorIndex = content.IndexOf(" a:");

        if (optionSeparatorIndex < 0)
        {
            throw new FormatException(
                $"Aucune réponse trouvée pour la question : {line}");
        }

        var questionText = content[..optionSeparatorIndex].Trim();
        var optionsText = content[optionSeparatorIndex..].Trim();

        var options = ParseOptions(optionsText);

        return new Question
        {
            Id = questionId,
            Text = questionText,
            Options = options
        };
    }

    private static List<AnswerOption> ParseOptions(string content)
    {
        var options = new List<AnswerOption>();

        var parts = content.Split(
            ',',
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var separatorIndex = part.IndexOf(':');

            if (separatorIndex <= 0)
            {
                throw new FormatException(
                    $"Format de réponse invalide : {part}");
            }

            var code = part[..separatorIndex].Trim();
            var text = part[(separatorIndex + 1)..].Trim();

            if (string.IsNullOrWhiteSpace(code) ||
                string.IsNullOrWhiteSpace(text))
            {
                throw new FormatException(
                    $"Une réponse doit avoir un code et une valeur : {part}");
            }

            options.Add(new AnswerOption
            {
                Code = code,
                Text = text
            });
        }

        return options;
    }
}