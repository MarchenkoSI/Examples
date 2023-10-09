using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum PlayerAbilitySlot
{
    Slot01,
    Slot02,
    Slot03
}

public class AbilitySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image abilityIcon;
    [SerializeField] private Sprite defaultIcon;

    [SerializeField] private GameObject level;
    [SerializeField] private TextMeshProUGUI levelOrCountText;
    [SerializeField] private LocalizedString levelLocalized;

    [SerializeField] private PlayerAbilitySlot playerAbilitySlot;

    [SerializeField] private Transform dragParent;
    [SerializeField] private Canvas abilityCanvas;

    [SerializeField] private Transform parent;

    public Action<AbilitySlot> OnAbilityChanged;

    private void OnEnable()
    {
        HandleEnebledOrChanged();
    }

    private void HandleEnebledOrChanged()
    {
        Player player = ModuleManager.getInstance().GetPlayerManager().GetPlayer();

        switch (playerAbilitySlot)
        {
            case PlayerAbilitySlot.Slot01:
                PrepareAbilitySlot(AbilityDatabase.Instance.GetByUUID(player.firstAbilityUUID));
                break;
            case PlayerAbilitySlot.Slot02:
                PrepareAbilitySlot(AbilityDatabase.Instance.GetByUUID(player.secondAbilityUUID));
                break;
            case PlayerAbilitySlot.Slot03:
                PrepareAbilitySlot(AbilityDatabase.Instance.GetByUUID(player.thirdAbilityUUID));
                break;
        }
    }

    public void PrepareAbilitySlot(Ability ability)
    {
        Player player = ModuleManager.getInstance().GetPlayerManager().GetPlayer();

        if (ability == null)
        {
            currentAbility = null;
            level.SetActive(false);
            abilityIcon.sprite = defaultIcon;
            
            ModuleManager.getInstance().GetAbilityManager().RemoveAbilityFromPlayer(playerAbilitySlot);
        }
        else
        {
            currentAbility = ability;

            abilityIcon.sprite = currentAbility.abilityIcon;

            ModuleManager.getInstance().GetAbilityManager().AddAbilityToPlayer(currentAbility.UUID, playerAbilitySlot);
            
            FillLevelOrCount();
        }

        if (OnAbilityChanged == null) return;

        OnAbilityChanged.Invoke(this);
    }

    public Ability GetAbility()
    {
        return currentAbility;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragIcon = abilityIcon;

        dragIcon.transform.SetParent(dragParent);
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragIcon.rectTransform.anchoredPosition += eventData.delta / abilityCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {       
        AbilitySlot abilitySlot = eventData.pointerEnter?.GetComponent<AbilitySlot>();

        if (abilitySlot == null)
        {
            dragIcon.transform.SetParent(parent);
            dragIcon.transform.localPosition = Vector3.zero;
            PrepareAbilitySlot(null);
        }
        else
        {
            dragIcon.transform.SetParent(parent);
            dragIcon.transform.localPosition = Vector3.zero;

            abilitySlot.PrepareAbilitySlot(currentAbility);
        }
    }

    private void FillLevelOrCount()
    {
        level.SetActive(true);

        if (currentAbility.abilytyType == AbilytyType.Potion)
        {
            if (Inventory.Instance.GetItemCount(currentAbility.parentItem.UUID) <= 0)
            {
                ResetSlot();
            }
            else
            {
                levelOrCountText.text = $"{Inventory.Instance.GetItemCount(currentAbility.parentItem.UUID)}";
            }
        }
        else
        {
            levelOrCountText.text = string.Format(levelLocalized, currentAbility.level);
        }
    }

    private void ResetSlot()
    {
        ModuleManager.getInstance().GetAbilityManager().RemoveAbilityFromPlayer(playerAbilitySlot);

        currentAbility = null;

        HandleEnebledOrChanged();
    }

    private Image dragIcon;

    private Ability currentAbility = null;
}
