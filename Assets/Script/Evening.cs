using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/Evening")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "Evening", message: "Evening", category: "Events", id: "256d3fa9e0eedf4e0c3d6491e117b9f0")]
public sealed partial class Evening : EventChannel { }

