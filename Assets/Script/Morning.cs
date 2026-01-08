using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Morning")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Morning", message: "Morning", category: "Events", id: "a2f5925e80363c4991ac006e17847e5b")]
public sealed partial class Morning : EventChannel { }

