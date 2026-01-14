using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Night")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Night", message: "Night", category: "Events", id: "6e8ccb8497e0cfb6436ffa2a1251930f")]
public sealed partial class Night : EventChannel { }

