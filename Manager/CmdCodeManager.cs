public enum EnumCmdCode
{
    login = 0x0000,
    logout = 0x0001,
    register = 0x0002,

    equipment = 0x0010,
    equipmentAttributes = 0x0011,
    inventory = 0x0012,
    inventoryAttributes = 0x0013,

    equipItem0 = 0x0014,

    syncMobsData = 0x0100,
    syncMobsDeadData = 0x0101,

    syncCallBack = 0x0140,
    syncPlayerData = 0x0150,

    mobsAttackPlayer = 0x0200,
    mobsHeal = 0x0201,
    mobsInjured = 0x0202,
    mobsDie = 0x0203,
    mobsAttackOtherPlayer = 0x0204,

    updateHP = 0x0300,
    updateMP = 0x0301,

    playerAttackMob = 0x0400,
    otherPlayerAttackMob = 0x0401,
}