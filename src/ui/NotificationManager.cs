using System;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui
{
	internal class NotificationManager : MonoBehaviour
	{
		public readonly List<Notification> notifications = new List<Notification>();
		public bool disableNotifications = false;

		public static Vector2 BoxSize
		{
			get { return new Vector2(325, 90) * MainUI.scale; }
		}

		public static Vector2 BoxHeaderSize
		{
			get { return new Vector2(BoxSize.x, 17 * MainUI.scale); }
		}

		public static Vector2 BoxContentPadding
		{
			get { return new Vector2(10, 0) * MainUI.scale; }
		}

		public static Vector2 BoxContentSize
		{
			get { return new Vector2(BoxSize.x - BoxContentPadding.x, BoxSize.y - BoxHeaderSize.y - BoxSliderSize.y); }
		}

		public static Vector2 BoxSliderSize
		{
			get { return new Vector2(BoxSize.x, 20 * MainUI.scale); }
		}

		public void Update()
		{
			int notificationCount = Math.Min(GetMaxNotifications(), notifications.Count);

			for(int i = 0; i < notificationCount; i++)
			{
				Notification notification = notifications[i];
				notification.lifetime += Time.deltaTime;

				if(notification.HasExpired)
				{
					notifications.RemoveAt(i);

					// 因为我们从通知列表中移除了一个元素，必须同时减少当前通知索引
					// 和最大通知数，以避免访问超出列表长度的错误
					i--;
					notificationCount--;
					continue;
				}
			}
		}

		public void OnGUI()
		{
			if(disableNotifications) return;

			int notificationCount = Math.Min(GetMaxNotifications(), notifications.Count);

			for(byte i = 0; i < notificationCount; i++)
			{
				RenderNotification(i, notifications[i]);
			}
		}

		private void RenderNotification(byte position, Notification notification)
		{
			float boxX = Screen.width - BoxSize.x;
			float boxY = Screen.height - (int)(BoxSize.y * (position + 1));

			GUI.Box(new Rect(boxX, boxY, BoxSize.x, BoxSize.y), notification.title);

			GUI.Label(new Rect(boxX + BoxContentPadding.x, boxY + BoxHeaderSize.y, BoxContentSize.x, BoxContentSize.y), notification.message);

			GUI.HorizontalSlider(new Rect(boxX, boxY + BoxHeaderSize.y + BoxContentSize.y, BoxSize.x, BoxSize.y), notification.ttl - notification.lifetime, 0, notification.ttl);
		}

		public int GetMaxNotifications()
		{
			return Screen.height / 2 / (int)BoxSize.y;
		}

		// 通知的存活时间值：成功消息为 5 秒，失败消息为 10 秒
		public void Send(string title, string message, float ttl = 10)
		{
			Hydra.Log.LogMessage($"[通知] [{title}] {message}");

			if(disableNotifications) return;

			Notification notification = new Notification(title, message, ttl);
			notifications.Add(notification);
		}

		public void ClearNotifications()
		{
			notifications.Clear();
		}
	}
}