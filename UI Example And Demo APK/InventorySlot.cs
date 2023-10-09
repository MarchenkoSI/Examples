using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IDropHandler
{
    public int slotId;

    public ItemInfo item;
    public int count;
    public Image itemIcon;
    public TextMeshProUGUI countText;
   
    public InventoryManager inventoryManager;

    private void OnEnable()
    {
        SetItemToSlot();

        Inventory.Instance.OnInventoryChanged.AddListener(SetItemToSlot);
    }

    private void OnDisable()
    {
        Inventory.Instance.OnInventoryChanged.RemoveListener(SetItemToSlot);
    }

    public void SetItemToSlot() 
    {
        item = Inventory.Instance.Get(slotId);

        ProcessingItemCount();

        Color color = itemIcon.color;

        if (item == null)
        {
            color.a = 0;
            itemIcon.color = color;
        }
        else
        {
            itemIcon.sprite = item.Icon;
            color.a = 1;
            itemIcon.color = color;
        }
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (item == null)
        {
            inventoryManager.ShowItemDescription(null, slotId);
            countText.gameObject.SetActive(false);
        }
        else
        {
            inventoryManager.ShowItemDescription(item, slotId);
        }        
    }

    public void OnBeginDrag(PointerEventData data)
    {
        if (item == null) return;

        if (itemIcon.sprite == null) return;

        itemIcon.transform.SetParent(inventoryManager.parantForDrag);
    }

    public void OnDrag(PointerEventData data)
    {
        if (item == null) return;

        if (itemIcon.sprite == null) return;

        itemIcon.rectTransform.anchoredPosition += data.delta / inventoryManager.inventoryCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData data)
    {
        if (itemIcon.sprite == null) return;

        itemIcon.transform.SetParent(gameObject.transform);
        itemIcon.transform.position = gameObject.transform.position;

        if (data.pointerEnter.gameObject.GetComponent<InventorySlot>() != null) return;

        Color color = itemIcon.color;
        color.a = 1;
        itemIcon.color = color;        
    }

    public void OnDrop(PointerEventData data)
    {
        InventorySlot drop_item = data.pointerPress.GetComponent<InventorySlot>();
        int count = data.pointerPress.GetComponent<InventorySlot>().count;

        if (drop_item.item == null) return;

        if (item == null)
        {
            Inventory.Instance.AddItemToSlot(drop_item.item, slotId, count);
            Inventory.Instance.RemoveItemFromSlot(drop_item.slotId);
        }
        else
        {
            //ItemInfo myItem = item;
            AddItemIfItenEquelItem(drop_item, slotId, count);
            //Inventory.Instance.AddItemToSlot(drop_item.item, slotId, count);
            //Inventory.Instance.AddItemToSlot(myItem, drop_item.slotId);
        }

        inventoryManager.ShowItemDescription(item, slotId);
    }

    private void ProcessingItemCount()
    {
        countText.gameObject.SetActive(false);

        if (item == null) return;

        if (item.stackSize == 1)
        {
            countText.gameObject.SetActive(false);
            count = 1;
        }
        else
        {
            if (Inventory.Instance.GetItemCount(slotId) == -1) return;
            count = Inventory.Instance.GetItemCount(slotId);
            countText.text = $"{count}";
            countText.gameObject.SetActive(true);
        }
    }

    private void AddItemIfItenEquelItem(InventorySlot dropItem, int slotIndex, int count)
    {
        if (dropItem.item.UUID == Inventory.Instance.Get(slotIndex).UUID)
        {
            Debug.Log("Item == item");

            ItemInfo myItem = item;
            ItemInfo newItem = dropItem.item;

            int dropItemSlotId = dropItem.slotId;
            int dropItemCount = count;
            int myItemCount = Inventory.Instance.GetItemCount(slotId);

            int myItemNewCount = myItemCount + dropItemCount;

            if (myItemNewCount <= item.stackSize)
            {
                Inventory.Instance.RemoveItemFromSlot(slotId);
                Inventory.Instance.RemoveItemFromSlot(dropItemSlotId);

                Inventory.Instance.AddItemToSlot(newItem, slotId, myItemNewCount);
            }
            else
            {
                int extra = myItemNewCount - item.stackSize;

                Inventory.Instance.RemoveItemFromSlot(slotId);
                Inventory.Instance.RemoveItemFromSlot(dropItemSlotId);

                Inventory.Instance.AddItemToSlot(newItem, slotId, newItem.stackSize);
                Inventory.Instance.AddItemToSlot(myItem, dropItemSlotId, extra);
            }
        }
        else
        {
            ItemInfo myItem = item;
            ItemInfo newItem = dropItem.item;

            int dropItemSlotId = dropItem.slotId;
            int dropItemCount = count;
            int myItemCount = Inventory.Instance.GetItemCount(slotId);

            Inventory.Instance.RemoveItemFromSlot(slotId);
            Inventory.Instance.RemoveItemFromSlot(dropItemSlotId);
            
            Inventory.Instance.AddItemToSlot(newItem, slotId, dropItemCount);
            Inventory.Instance.AddItemToSlot(myItem, dropItemSlotId, myItemCount);
        }
    }
}
