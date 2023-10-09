using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Inventory", menuName = "Database/Inventory")]
public class Inventory : ScriptableObject
{
	private static Inventory instance;

	public static Inventory Instance
	{
		get
		{
			if (instance == null)
				instance = Resources.Load("Databases/Inventory") as Inventory;

			return instance;
		}
	}

	public ItemInfo Get(int index)
	{
		return (this.inventorySlots[index]);
	}

	public ItemInfo GetItemFromInventoryByUUID(string UUID)
    {
		foreach (ItemInfo item in inventorySlots)
        {
			if (item == null) continue;

			if (item.UUID != UUID) continue;

			return item;
        }

		return null;
    }

    public void AddItem(ItemInfo item, int count)
	{
		if (item == null) return;

		int slotIndex = FindItemInInventory(item);

		if (slotIndex == -1)
		{
			for (int slot_index = 0; slot_index < inventorySlots.Count; ++slot_index)
			{
				if (inventorySlots[slot_index] != null) continue;

				inventorySlots[slot_index] = item;

				ItemAndCount itemAndCount = new();
				itemAndCount.item = item.UUID;
				itemAndCount.count += count;

				inventory[slot_index] = itemAndCount;

				break;
			}
		}
		else
		{
			inventory[slotIndex].count += count;
		}

		SaveInventory();

		OnInventoryChanged.Invoke();
	}

	public void AddItemToSlot(ItemInfo item, int slotId)
	{
		inventorySlots[slotId] = item;

		ItemAndCount itemAndCount = new();
		itemAndCount.item = item.UUID;
		itemAndCount.count++;

		inventory[slotId] = itemAndCount;

		SaveInventory();

		OnInventoryChanged.Invoke();
	}

	public void AddItemToSlot(ItemInfo item, int slotId, int count)
    {
		inventorySlots[slotId] = item;

		ItemAndCount itemAndCount = new();
		itemAndCount.item = item.UUID;
		itemAndCount.count = count;

		inventory[slotId] = itemAndCount;

		SaveInventory();

		OnInventoryChanged.Invoke();
	}

	public void RemoveItemFromSlot(int slotId)
	{
		inventorySlots[slotId]	= null;
		inventory[slotId]		= null;

		SaveInventory();

		OnInventoryChanged.Invoke();
	}

	public void RemoveItemFromStuck(int slotId, int count)
	{
		inventory[slotId].count -= count;

		if (inventory[slotId].count <= 0)
        {
			RemoveItemFromSlot(slotId);

			return;
		}

		SaveInventory();

		OnInventoryChanged.Invoke();
	}

	public void RemoveItemFromStuckByUUID(string UUID, int count)
    {
		int slotID = -1;

		for (int index = 0; index < inventorySlots.Count; ++index)
        {
			if (inventorySlots[index] == null) continue;

			if (inventorySlots[index].UUID != UUID) continue;

			slotID = index;

			break;
		}

		if (slotID == -1) return;

		RemoveItemFromStuck(slotID, count);
	}

	public void LoadInventory()
	{
		inventory = ModuleManager.getInstance().GetPlayerManager().GetPlayer().itemsUUIDandCount;

		for (int slot_index = 0; slot_index < inventory.Count; ++slot_index)
		{
			inventorySlots.Add(ItemDatabase.Instance.GetByUUID(inventory[slot_index].item));
		}
	}

	public int GetItemCount(int slotIndex)
    {
		if (inventory[slotIndex] == null) return -1;

		if (inventory[slotIndex].item == null) return -1;

		return inventory[slotIndex].count;
    }

	public int GetItemCount(string UUID)
	{
		int count = 0;

		foreach (ItemAndCount itemsAndCount in inventory)
		{
			if (itemsAndCount == null) continue;

			if (itemsAndCount.item != UUID) continue;

			count += itemsAndCount.count;
		}

		return count;
	}

	public List<ItemAndCount> GetItems()
    {
		return inventory;
    }

	public void SaveInventory()
	{
		ModuleManager.getInstance().GetPlayerManager().GetPlayer().itemsUUIDandCount = inventory;
	}

	public List<ItemInfo> GetItemsWithAbility()
    {
		List<ItemInfo> itemsWithAbilities = new();

		foreach (ItemAndCount itemsAndCount in inventory)
        {
			if (itemsAndCount == null) continue;

			if (ItemDatabase.Instance.GetByUUID(itemsAndCount.item) == null) continue;

			if (ItemDatabase.Instance.GetByUUID(itemsAndCount.item).itemType != ItemType.CanUse) continue;

			if (ItemDatabase.Instance.GetByUUID(itemsAndCount.item).ability == null) continue;

			if (itemsWithAbilities.Contains(ItemDatabase.Instance.GetByUUID(itemsAndCount.item))) continue;

			itemsWithAbilities.Add(ItemDatabase.Instance.GetByUUID(itemsAndCount.item));
		}

		return itemsWithAbilities;
    }



	public List<ItemAndCount> CreateInventory()
	{
		for (int slot_index = 0; slot_index < MainConfig.NUMBER_OF_INVENTORY_SLOTS; ++slot_index)
		{
			inventory.Add(null);
			inventorySlots.Add(null);
		}

		return inventory;
	}

	public void EquipItem(ItemInfo item, int slotId)
	{
		switch (item.EquipType)
		{
			case EquipmentType.None:
				// Error
				break;
			case EquipmentType.OneHand:
				EquipOneHand(item, slotId);
				break;
			case EquipmentType.TwoHand:
				EquipTwoHand(item, slotId);
				break;
			case EquipmentType.OffHand:
				EquipOffHand(item, slotId);
				break;
			case EquipmentType.Head:
				EquipHead(item, slotId);
				break;
			case EquipmentType.Necklace:
				EquipNecklace(item, slotId);
				break;
			case EquipmentType.Shoulders:
				EquipShoulders(item, slotId);
				break;
			case EquipmentType.Chest:
				EquipChest(item, slotId);
				break;
			case EquipmentType.Back:
				EquipBack(item, slotId);
				break;
			case EquipmentType.Gloves:
				EquipGloves(item, slotId);
				break;
			case EquipmentType.Bracers:
				EquipBracers(item, slotId);
				break;
			case EquipmentType.Pants:
				EquipPants(item, slotId);
				break;
			case EquipmentType.Boots:
				EquipBoots(item, slotId);
				break;
			case EquipmentType.Finger:
				EquipFinger(item, slotId);
				break;
		}

		//RecalculateEquipmentStats();

		OnEquipmentChanged.Invoke();
	}

	public void TakeOffItem(ItemInfo item)
    {
		if (item == null) return;

		switch (item.EquipType)
		{
			case EquipmentType.None:
				// Error
				break;
			case EquipmentType.OneHand:
				ModuleManager.getInstance().GetPlayerManager().GetPlayer().mainHand = null;
				break;
			case EquipmentType.TwoHand:
				ModuleManager.getInstance().GetPlayerManager().GetPlayer().mainHand = null;
				break;
			case EquipmentType.OffHand:
				ModuleManager.getInstance().GetPlayerManager().GetPlayer().offHand = null;
				break;
			case EquipmentType.Head:
				ModuleManager.getInstance().GetPlayerManager().GetPlayer().head = null;
				break;
			case EquipmentType.Necklace:
				ModuleManager.getInstance().GetPlayerManager().GetPlayer().necklace = null;
				break;
			case EquipmentType.Shoulders:
				ModuleManager.getInstance().GetPlayerManager().GetPlayer().shoulders = null;
				break;
			case EquipmentType.Chest:
				ModuleManager.getInstance().GetPlayerManager().GetPlayer().chest = null;
				break;
			case EquipmentType.Back:
				ModuleManager.getInstance().GetPlayerManager().GetPlayer().back = null;
				break;
			case EquipmentType.Gloves:
				ModuleManager.getInstance().GetPlayerManager().GetPlayer().gloves = null;
				break;
			case EquipmentType.Bracers:
				ModuleManager.getInstance().GetPlayerManager().GetPlayer().bracers = null;
				break;
			case EquipmentType.Pants:
				ModuleManager.getInstance().GetPlayerManager().GetPlayer().pants = null;
				break;
			case EquipmentType.Boots:
				ModuleManager.getInstance().GetPlayerManager().GetPlayer().boots = null;
				break;
			case EquipmentType.Finger:
				ModuleManager.getInstance().GetPlayerManager().GetPlayer().finger = null;
				break;
		}

		Inventory.Instance.AddItem(item, 1);

		OnEquipmentChanged.Invoke();
	}

	public void ClearInventory()
    {
		inventorySlots.Clear();
		inventory.Clear();

		mainHand	= null;
		offHand		= null;
		head		= null;
		necklace	= null;
		shoulders	= null;
		chest		= null;
		back		= null;
		gloves		= null;
		bracers		= null;
		pants		= null;
		boots		= null;
		finger		= null;

		instance = null;
	}

	private int FindItemInInventory(ItemInfo item)
    {
		int slotIndex = -1;

		for (int index = 0; index < inventory.Count; ++index)
        {
			if (inventory[index] == null) continue;

			if (inventory[index].item != item.UUID) continue;

			if (item.stackSize <= inventory[index].count) continue;

			slotIndex = index;

			break;
		}

		return slotIndex;
	}

    #region Equip
    private void EquipOneHand(ItemInfo item, int slotId)
	{
		Player player = ModuleManager.getInstance().GetPlayerManager().GetPlayer();

		if (player.mainHand == "" || player.mainHand == null)
        {
			player.mainHand = item.UUID;

			RemoveItemFromSlot(slotId);
		}
        else
        {
			ItemInfo tempItem = ItemDatabase.Instance.GetByUUID(player.mainHand);

			player.mainHand = item.UUID;

			RemoveItemFromSlot(slotId);

			AddItem(tempItem, 1);
		}
    }
	private void EquipTwoHand(ItemInfo item, int slotId)
	{
		Player player = ModuleManager.getInstance().GetPlayerManager().GetPlayer();

		if (player.mainHand == "" || player.mainHand == null)
		{
			player.mainHand = item.UUID;

			RemoveItemFromSlot(slotId);
		}
		else
		{
			ItemInfo tempItem = ItemDatabase.Instance.GetByUUID(player.mainHand);

			player.mainHand = item.UUID;

			RemoveItemFromSlot(slotId);

			AddItem(tempItem, 1);
		}

		if (player.offHand == "" || player.mainHand == null) return;

		AddItem(ItemDatabase.Instance.GetByUUID(player.offHand), 1);

		player.offHand = null;
	}
	private void EquipOffHand(ItemInfo item, int slotId)
	{
		Player player = ModuleManager.getInstance().GetPlayerManager().GetPlayer();

		if (player.offHand == "" || player.offHand == null)
		{
			player.offHand = item.UUID;

			RemoveItemFromSlot(slotId);
		}
		else
		{
			ItemInfo tempItem = ItemDatabase.Instance.GetByUUID(player.offHand);

			player.offHand = item.UUID;

			RemoveItemFromSlot(slotId);

			AddItem(tempItem, 1);
		}

		if (player.mainHand == "" || player.mainHand == null) return;

		if (ItemDatabase.Instance.GetByUUID(player.mainHand).EquipType != EquipmentType.TwoHand) return;

		AddItem(ItemDatabase.Instance.GetByUUID(player.mainHand), 1);

		player.mainHand = null;
	}
	private void EquipHead(ItemInfo item, int slotId)
	{

	}
	private void EquipNecklace(ItemInfo item, int slotId)
	{

	}
	private void EquipShoulders(ItemInfo item, int slotId)
	{

	}
	private void EquipChest(ItemInfo item, int slotId)
	{
		Player player = ModuleManager.getInstance().GetPlayerManager().GetPlayer();

		if (player.chest == "" || player.chest == null)
		{
			player.chest = item.UUID;

			RemoveItemFromSlot(slotId);
		}
		else
		{
			ItemInfo tempItem = ItemDatabase.Instance.GetByUUID(player.chest);

			player.chest = item.UUID;

			RemoveItemFromSlot(slotId);

			AddItem(tempItem, 1);
		}
	}
	private void EquipBack(ItemInfo item, int slotId)
	{

	}
	private void EquipGloves(ItemInfo item, int slotId)
	{

	}
	private void EquipBracers(ItemInfo item, int slotId)
	{

	}
	private void EquipPants(ItemInfo item, int slotId)
	{
		Player player = ModuleManager.getInstance().GetPlayerManager().GetPlayer();

		if (player.pants == "" || player.pants == null)
		{
			player.pants = item.UUID;

			RemoveItemFromSlot(slotId);
		}
		else
		{
			ItemInfo tempItem = ItemDatabase.Instance.GetByUUID(player.pants);

			player.pants = item.UUID;

			RemoveItemFromSlot(slotId);

			AddItem(tempItem, 1);
		}
	}
	private void EquipBoots(ItemInfo item, int slotId)
	{
		Player player = ModuleManager.getInstance().GetPlayerManager().GetPlayer();

		if (player.boots == "" || player.boots == null)
		{
			player.boots = item.UUID;

			RemoveItemFromSlot(slotId);
		}
		else
		{
			ItemInfo tempItem = ItemDatabase.Instance.GetByUUID(player.boots);

			player.boots = item.UUID;

			RemoveItemFromSlot(slotId);

			AddItem(tempItem, 1);
		}
	}
	private void EquipFinger(ItemInfo item, int slotId)
	{

	}
	#endregion Equip

	public UnityEvent OnInventoryChanged;
	public UnityEvent OnEquipmentChanged;

	private readonly List<ItemInfo> inventorySlots = new List<ItemInfo>();

	private List<ItemAndCount> inventory = new();

	private ItemInfo mainHand;
	private ItemInfo offHand;
	private ItemInfo head;
	private ItemInfo necklace;
	private ItemInfo shoulders;
	private ItemInfo chest;
	private ItemInfo back;
	private ItemInfo gloves;
	private ItemInfo bracers;
	private ItemInfo pants;
	private ItemInfo boots;
	private ItemInfo finger;
}
