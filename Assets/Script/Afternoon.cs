using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Afternoon")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Afternoon", message: "Afternoon", category: "Events", id: "c8972277fce49699eff90b6da7aaac6c")]
public sealed partial class Afternoon : EventChannel { }

