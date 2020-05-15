using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Rendering;

public class UIManager : MonoBehaviour
{
    [Header("References - General")] 
    public PlayerMovement playerMovement;
    public GameManager gameManager;
    [Header("References - Money Display")]
    public GameObject moneyUIContainer;
    private TextMeshProUGUI _moneyText;
    [Header("References - Fuel Display")]
    public GameObject fuelBarContainer;
    public Image fuelBarImage;
    [Header("References - Navigator Display")]
    public GameObject navigatorContainer;
    public Image navigatorImage;
    public TextMeshProUGUI navigatorDistText;
    [Header("References - Day Info Display")]
    public GameObject dayInfoContainer;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI currentDayText;
    public TextMeshProUGUI questText;
    [Header("References - Message Display")]
    public GameObject messageContainer;
    public TextMeshProUGUI messageUI;
    [Header("References - Transition Screen")]
    public GameObject transitionContainer;
    public TextMeshProUGUI transitionTitleUI;
    public TextMeshProUGUI moneyBankedUI;
    public TextMeshProUGUI distanceUI;
    public TextMeshProUGUI deliveriesUI;
    public TextMeshProUGUI buttonPromptUI;
    
    private Vector3 _initFuelBarPos;
    private Vector3 _initMoneyPos;
    private Vector3 _initNavPos;
    private Vector3 _initDayInfoPos;
    private Vector3 _initMessagePos;
    private Vector3 _initTransitionPos;

    // TODO: Moving the elements by arbitrary numbers is no good -> use screen width/height in the future
    // Start is called before the first frame update
    private void Start()
    {
        _moneyText = moneyUIContainer.GetComponent<TextMeshProUGUI>();

        _initFuelBarPos = fuelBarContainer.transform.position;
        _initMoneyPos = moneyUIContainer.transform.position;
        _initNavPos = navigatorContainer.transform.position;
        _initDayInfoPos = dayInfoContainer.transform.position;
        _initMessagePos = messageContainer.transform.position;
        _initTransitionPos = transitionContainer.transform.position;
       
        ShowFuelBar(false);
        ShowMoney(false);
        ShowNavigator(false);
        ShowDayInfo(false);
        ShowTransitionScreen(false);
       
        // This is only for the purposes of the game jam version
        // TODO: Make this better
        DisplayMessage("Interact with the yellow terminal to start the day!"); 
    }

    private void Update()
    {
        
    }

    public void ShowFuelBar(bool show)
    {
        if (show)
        {
            fuelBarContainer.transform.DOMoveY(_initFuelBarPos.y, .3f).SetEase(Ease.OutBack);
        }
        else
        {
            fuelBarContainer.transform.DOMoveY(_initFuelBarPos.y - (Screen.height), .3f);
        }
    }

    public void ShowMoney(bool show)
    {
        if (show)
        {
            moneyUIContainer.transform.DOMoveX(_initMoneyPos.x, .3f).SetEase(Ease.OutBack);
        }
        else
        {
            moneyUIContainer.transform.DOMoveX(_initMoneyPos.x - (Screen.width), .3f);
        }
    }

    public void ShowNavigator(bool show)
    {
        if (show)
        {
            navigatorContainer.transform.DOMoveY(_initNavPos.y, .3f).SetEase(Ease.OutBack);
        }
        else
        {
            navigatorContainer.transform.DOMoveY(_initNavPos.y - (Screen.height), .3f);
        }
    }

    public void ShowDayInfo(bool show)
    {
        if (show)
        {
            dayInfoContainer.transform.DOMoveY(_initDayInfoPos.y, .3f).SetEase(Ease.OutBack);
        }
        else
        {
            dayInfoContainer.transform.DOMoveY(_initDayInfoPos.y + (Screen.height), .3f);
        }
    }

    public void ShowMessageUI(bool show)
    {
        if (show)
        {
            messageContainer.transform.DOMoveY(_initMessagePos.y, .3f).SetEase(Ease.OutBack);
        }
        else
        {
            messageContainer.transform.DOMoveY(_initMessagePos.y - (Screen.height), .3f);
            // If we're hiding this -> clear the message too
            DisplayMessage("");
        }
    }

    public void ShowTransitionScreen(bool show)
    {
        if (show)
        {
            transitionContainer.transform.DOMoveX(_initTransitionPos.x, .3f).SetEase(Ease.OutBack);
        }
        else
        {
            transitionContainer.transform.DOMoveX(_initTransitionPos.x - (Screen.width * 1.5f), .3f);
        }
    }

    // Used for one time transactions
    public void UpdateMoneyCount(float newValue, float hideTime)
    {
        ShowMoney(true);
        _moneyText.text = "$" + Mathf.Round(newValue).ToString();
        StartCoroutine(HideMoneyAfter(3));
    }
    
    // Used for more constant updates
    public void UpdateMoneyCount(float newValue)
    {
        _moneyText.text = "$" + Mathf.Round(newValue).ToString();
    }

    public void UpdateDistance(float distance)
    {
        navigatorDistText.text = Mathf.Round(distance).ToString() + "m";
    }
    
    public void UpdateFuelBarUI(float newValue)
    {
        fuelBarImage.fillAmount = newValue / 100f;
    }

    public void UpdateTimer(float timeLeft)
    {
        var minutesText = Mathf.FloorToInt(timeLeft / 60).ToString();
        var seconds = Mathf.FloorToInt(timeLeft % 60);
        var secondsText = seconds >= 10 ? seconds.ToString() : ("0" + seconds.ToString());
        
        timerText.text = (minutesText + ":" + secondsText);
    }

    public void UpdateDay(int day, int maxDays)
    {
        currentDayText.text = ("Day: " + day.ToString() + " of " + maxDays.ToString());
    }

    public void UpdateQuestCount(int currentQuestCount, int maxQuests)
    {
        questText.text = ("Delivery: " + (currentQuestCount+1).ToString() + " of " + maxQuests.ToString());
    }

    public void UpdateNavigator(Vector3 direction, float distance, Vector3 heading)
    {
        var angle = -Mathf.Atan2(heading.x, heading.z) * Mathf.Rad2Deg;

        navigatorImage.GetComponent<RectTransform>().transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        
        UpdateDistance(distance); 
    }

    public void DisplayMessage(string msg)
    {
        messageUI.text = msg;
    }

    public void DisplayMessage(string msg, float hideTime)
    {
       ShowMessageUI(true);
       messageUI.text = msg;
       StartCoroutine(HideMessageAfter(hideTime));
    }

    public void UpdateTransitionScreen(TransitionType type, float moneyBanked, float distance, float packageCount, int day)
    {
        ShowTransitionScreen(true);
        // Set title
        switch (type)
        {
            case TransitionType.NextDay:
                transitionTitleUI.text = "Day " + day.ToString() + " Completed!";
                buttonPromptUI.text = @"Press ""Interact"" to continue!";
                break;
            case TransitionType.GameFinished:
                transitionTitleUI.text = "Week Completed! (ggwp, thanks for playing)";
                buttonPromptUI.text = @"Press ""Interact"" to play again!";
                break;
            case TransitionType.GameOver:
                transitionTitleUI.text = "Game Over - You ran out of time...";
                buttonPromptUI.text = @"Press ""Interact"" to try again!";
                break;
        }
        // TODO: Animate these nicely?
        moneyBankedUI.text = "$"+Mathf.Floor(moneyBanked).ToString();
        distanceUI.text = Mathf.Floor(distance).ToString() + "m";
        deliveriesUI.text = packageCount.ToString();
    }
    
    private IEnumerator HideMoneyAfter(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        
        ShowMoney(false);
    }

    private IEnumerator HideMessageAfter(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        
        ShowMessageUI(false);
    }
}
