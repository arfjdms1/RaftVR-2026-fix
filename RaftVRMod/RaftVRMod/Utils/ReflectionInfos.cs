using HarmonyLib;
using System.Reflection;
using UltimateWater;
using UnityEngine;

namespace RaftVR.Utils
{
    static class ReflectionInfos
    {
        // ── Safe wrapper helpers ──────────────────────────────────────────
        // These prevent NullReferenceException cascades in Harmony transpilers
        // when Raft renames or removes private members in a game update.

        private static FieldInfo SafeField(System.Type type, string name)
        {
            FieldInfo field = AccessTools.Field(type, name);
            if (field == null)
                Debug.LogWarning($"[RaftVR] Reflection target not found: {type.Name}.{name} (field). A Raft update may have renamed it.");
            return field;
        }

        private static MethodInfo SafeMethod(System.Type type, string name)
        {
            MethodInfo method = AccessTools.Method(type, name);
            if (method == null)
                Debug.LogWarning($"[RaftVR] Reflection target not found: {type.Name}.{name} (method). A Raft update may have renamed it.");
            return method;
        }

        private static MethodInfo SafePropertySetter(System.Type type, string name)
        {
            MethodInfo setter = AccessTools.PropertySetter(type, name);
            if (setter == null)
                Debug.LogWarning($"[RaftVR] Reflection target not found: {type.Name}.{name} (property setter). A Raft update may have renamed it.");
            return setter;
        }

        // ── Fields ────────────────────────────────────────────────────────
        internal static FieldInfo chargeMeterCurrentChargeField = SafeField(typeof(ChargeMeter), "currentCharge");
        internal static FieldInfo chargeMeterMinChargeField = SafeField(typeof(ChargeMeter), "minCharge");
        internal static FieldInfo chargeMeterMaxChargeField = SafeField(typeof(ChargeMeter), "maxCharge");

        internal static FieldInfo displayTextPriorityField = SafeField(typeof(DisplayText), "currentPriority");
        internal static FieldInfo displayTextButtonTextField = SafeField(typeof(DisplayText), "buttonText");

        internal static FieldInfo throwableRotationField = SafeField(typeof(Throwable), "throwableStartRotation");

        internal static FieldInfo animationConnectionsField = SafeField(typeof(AnimationEventCaller), "connections");

        internal static FieldInfo toolOnPressUseEventField = SafeField(typeof(UsableTool), "OnPressUseButton");
        internal static FieldInfo toolOnReleaseUseEventField = SafeField(typeof(UsableTool), "OnReleaseUseButton");
        internal static FieldInfo toolThisItemField = SafeField(typeof(UsableTool), "thisItem");
        internal static FieldInfo toolSetAnimationField = SafeField(typeof(UsableTool), "setItemHitAnimation");

        internal static FieldInfo weaponDamageField = SafeField(typeof(MeleeWeapon), "damage");
        internal static FieldInfo weaponGoThroughInvurnabilityField = SafeField(typeof(MeleeWeapon), "goThroughInvurnability");

        internal static FieldInfo usableUseAnimationField = SafeField(typeof(ItemInstance_Usable), "animationOnUse");

        internal static FieldInfo netPickupTargetField = SafeField(typeof(SweepNet), "currentPickupTarget");
        internal static FieldInfo netSwingEventField = SafeField(typeof(SweepNet), "eventRef_netSwing");

        internal static FieldInfo itemCanChannelField = SafeField(typeof(UseableItem), "canChannel");

        internal static FieldInfo shovelCurrentTargetField = SafeField(typeof(Shovel), "currentTarget");

        internal static FieldInfo hookGatherTimerField = SafeField(typeof(Hook), "gatherTimer");
        internal static FieldInfo hookGatherEmitterMethod = SafeField(typeof(Hook), "eventEmitter_gather");

        internal static FieldInfo optionsMenuSettingsField = SafeField(typeof(OptionsMenuBox), "settings");

        internal static FieldInfo personControllerNetworkPlayerField = SafeField(typeof(PersonController), "playerNetwork");
        internal static FieldInfo personControllerCamTransformField = SafeField(typeof(PersonController), "camTransform");

        internal static FieldInfo macheteQuestTagField = SafeField(typeof(Machete), "macheteInteractTagName");

        internal static FieldInfo storageInventoryRefField = SafeField(typeof(Storage_Small), "inventoryReference");

        internal static FieldInfo characterModelPlayerNetworkField = SafeField(typeof(CharacterModelModifications), "playerNetwork");

        internal static FieldInfo throwableCanThrowField = SafeField(typeof(ThrowableComponent), "canThrow");

        internal static FieldInfo characterModelNetworkPlayerField = SafeField(typeof(CharacterModelModifications), "playerNetwork");

        internal static FieldInfo equipmentModelNetworkPlayerField = SafeField(typeof(Equipment_Model), "playerNetwork");

        // ── Methods ───────────────────────────────────────────────────────
        internal static MethodInfo usableItemUse = SafeMethod(typeof(UseItemController), "Use");

        internal static MethodInfo netAttemptCaptureMethod = SafeMethod(typeof(SweepNet), "AttemptCaptureWithNet");
        internal static MethodInfo netPlayCatureSoundMethod = SafeMethod(typeof(SweepNet), "PlaySuccessfullCaptureSound");

        internal static MethodInfo shovelResetMethod = SafeMethod(typeof(Shovel), "ResetItemChannel");

        internal static MethodInfo hookStartCollectingMethod = SafeMethod(typeof(Hook), "StartCollecting");
        internal static MethodInfo hookStopCollectingMethod = SafeMethod(typeof(Hook), "StopCollecting");
        internal static MethodInfo hookFinishGatheringMethod = SafeMethod(typeof(Hook), "FinishGathering");

        internal static MethodInfo macheteQuestInteract = SafeMethod(typeof(Machete), "MacheteInteractWithQuest");

        internal static MethodInfo waterDistortionField = SafePropertySetter(typeof(WaterMaterials), "UnderwaterDistortionsIntensity");

        internal static MethodInfo throwableReleaseHandMethod = SafeMethod(typeof(ThrowableComponent), "ReleaseHand");

        internal static MethodInfo ikSolverAnimatorSetMethod = SafePropertySetter(typeof(RootMotion.FinalIK.IKSolverVR), "animator");
    }
}
