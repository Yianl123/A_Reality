using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Educational quiz system with explanations for kids
/// Shows WHY answers are correct/incorrect
/// </summary>
public class QuizManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button[] answerButtons;
    
    [Header("Explanation Panel")]
    [SerializeField] private GameObject explanationPanel;
    [SerializeField] private Image explanationIcon;
    [SerializeField] private TextMeshProUGUI explanationTitle;
    [SerializeField] private TextMeshProUGUI explanationText;
    [SerializeField] private Button nextButton;
    
    [Header("Final Score Panel")]
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button closeButton;
    
    [Header("Visual Feedback")]
    [SerializeField] private Sprite correctIcon;
    [SerializeField] private Sprite incorrectIcon;
    [SerializeField] private Color correctColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color incorrectColor = new Color(0.9f, 0.3f, 0.3f);
    [SerializeField] private Color normalColor = Color.white;
    
    private CreatureData currentCreature;
    private QuizQuestion currentQuestion;
    private int currentQuestionIndex = 0;
    private int correctAnswers = 0;
    private List<QuizQuestion> shuffledQuestions = new List<QuizQuestion>();
    
    private void HideAllPanels()
    {
        if (quizPanel != null) 
        {
            quizPanel.SetActive(false);
            Debug.Log("Quiz panel hidden");
        }
        
        if (explanationPanel != null) 
        {
            explanationPanel.SetActive(false);
            Debug.Log("Explanation panel hidden");
        }
        
        if (scorePanel != null) 
        {
            scorePanel.SetActive(false);
            Debug.Log("Score panel hidden");
        }
    }
    private void Start()
    {
        HideAllPanels();
        
        // Setup buttons
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseQuiz);
        }
    }
    public void StartQuiz(CreatureData creature)
    {
        if (creature == null)
        {
            Debug.LogError("Cannot start quiz - creature is null!");
            return;
        }
        
        if (creature.quizQuestions == null || creature.quizQuestions.Count == 0)
        {
            Debug.Log($"No quiz questions available for {creature.displayName}");
            return;
        }
        
        currentCreature = creature;
        currentQuestionIndex = 0;
        correctAnswers = 0;
        
        shuffledQuestions = creature.quizQuestions.OrderBy(x => Random.value).ToList();

        // Hide others
        quizPanel.SetActive(true);
        explanationPanel.SetActive(false);
        scorePanel.SetActive(false);
        
        
        ShowNextQuestion();
        
        Debug.Log($"✓ Started quiz for {creature.displayName} with {creature.quizQuestions.Count} questions");
    }
    
    private void ShowNextQuestion()
    {
        if (currentQuestionIndex >= shuffledQuestions.Count)
        {
            ShowFinalScore();
            return;
        }
        
        currentQuestion = shuffledQuestions[currentQuestionIndex];
        questionText.text = currentQuestion.questionText;
        foreach (var btn in answerButtons)
        {
            if (btn != null)
            {
                btn.GetComponent<Image>().color = normalColor;
                btn.interactable = true;
            }
        }
        
        if (explanationPanel != null)
        {
            explanationPanel.SetActive(false);
        }
        
        switch (currentQuestion.type)
        {
            case QuestionType.TrueFalse:
                SetupTrueFalseButtons();
                break;
            case QuestionType.MultipleChoice:
                SetupMultipleChoiceButtons();
                break;
        }
    }
    
    private void SetupTrueFalseButtons()
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < 2)
            {
                answerButtons[i].gameObject.SetActive(true);
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
        
        answerButtons[0].GetComponentInChildren<TextMeshProUGUI>().text = "Đúng";
        answerButtons[1].GetComponentInChildren<TextMeshProUGUI>().text = "Sai";
        
        answerButtons[0].onClick.RemoveAllListeners();
        answerButtons[1].onClick.RemoveAllListeners();
        
        answerButtons[0].onClick.AddListener(() => CheckAnswer("True", answerButtons[0]));
        answerButtons[1].onClick.AddListener(() => CheckAnswer("False", answerButtons[1]));
    }
    
    private void SetupMultipleChoiceButtons()
    {
        List<string> shuffledOptions = currentQuestion.options.OrderBy(x => Random.value).ToList();
        
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < shuffledOptions.Count)
            {
                answerButtons[i].gameObject.SetActive(true);
                
                answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = shuffledOptions[i];
                
                string answer = shuffledOptions[i];
                Button btn = answerButtons[i];
                
                answerButtons[i].onClick.RemoveAllListeners();
                
                answerButtons[i].onClick.AddListener(() => CheckAnswer(answer, btn));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
        
        Debug.Log($"Shuffled options: {string.Join(", ", shuffledOptions)}");
        Debug.Log($"Correct answer: {currentQuestion.correctAnswer}");
    }
    
    private void CheckAnswer(string selectedAnswer, Button clickedButton)
    {
        bool isCorrect = selectedAnswer == currentQuestion.correctAnswer;
        
        // Disable all buttons to prevent multiple clicks
        foreach (var btn in answerButtons)
        {
            if (btn != null)
            {
                btn.interactable = false;
            }
        }
        
        if (isCorrect)
        {
            correctAnswers++;
            clickedButton.GetComponent<Image>().color = correctColor;
        }
        else
        {
            clickedButton.GetComponent<Image>().color = incorrectColor;
        }

        ShowExplanation(isCorrect);
    }
    
    private void ShowExplanation(bool isCorrect)
    {
        if (explanationPanel == null)
        {
            Debug.LogWarning("Explanation panel not assigned!");
            // Skip explanation and go to next question
            Invoke(nameof(OnNextButtonClicked), 2f);
            return;
        }
        
        explanationPanel.SetActive(true);
        
        if (explanationTitle != null)
        {
            if (isCorrect)
            {
                explanationTitle.text = "Chính xác!";
                explanationTitle.color = correctColor;
            }
            else
            {
                explanationTitle.text = "Chưa đúng...";
                explanationTitle.color = incorrectColor;
            }
        }
        
        if (explanationText != null)
        {
            if (string.IsNullOrEmpty(currentQuestion.explanation))
            {
                explanationText.text = isCorrect 
                ? "Tuyệt vời! Em đã trả lời đúng!" 
                : $"Đáp án đúng là: {currentQuestion.correctAnswer}";
            }
            else
            {
                explanationText.text = currentQuestion.explanation;
            }
        }
        
        Debug.Log($"{(isCorrect ? "✓" : "✗")} Answer: {(isCorrect ? "Correct" : "Incorrect")}");
    }
    
    private void OnNextButtonClicked()
    {
        currentQuestionIndex++;
        ShowNextQuestion();
    }
    
    private void ShowFinalScore()
    {
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }
        
        if (explanationPanel != null)
        {
            explanationPanel.SetActive(false);
        }
        
        if (scorePanel != null)
        {
            scorePanel.SetActive(true);
        }
        
        int total = shuffledQuestions.Count;
        float percentage = (float)correctAnswers / total * 100f;
        
        string resultMessage = $"Hoàn thành bài quiz!\n\n";
        resultMessage += $"Em trả lời đúng {correctAnswers}/{total} câu\n";
        resultMessage += $"({percentage:F0}%)\n\n";

        if (percentage == 100f)
        {
            resultMessage += "Hoàn hảo! Em là chuyên gia về " + currentCreature.displayName + "!";
        }
        else if (percentage >= 80f)
        {
            resultMessage += "Xuất sắc! Em biết rất nhiều về " + currentCreature.displayName + "!";
        }
        else if (percentage >= 60f)
        {
            resultMessage += "Tốt lắm! Tiếp tục học nhé!";
        }
        else
        {
            resultMessage += "Thử lại để học thêm về " + currentCreature.displayName + " nhé!";
        }
        
        if (scoreText != null)
        {
            scoreText.text = resultMessage;
        }
        
        Debug.Log($"✓ Quiz completed: {correctAnswers}/{total} correct ({percentage:F0}%)");
    }
    
    public void CloseQuiz()
    {
        // Hide all panels
        if (quizPanel != null) quizPanel.SetActive(false);
        if (explanationPanel != null) explanationPanel.SetActive(false);
        if (scorePanel != null) scorePanel.SetActive(false);
        
        Debug.Log("✓ Quiz closed");
    }
}