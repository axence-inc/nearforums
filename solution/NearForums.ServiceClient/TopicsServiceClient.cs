﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NearForums.DataAccess;

namespace NearForums.ServiceClient
{
	public static class TopicsServiceClient
	{
		public static List<Topic> GetByForum(int forumId, int startIndex, int length)
		{
			TopicsDataAccess da = new TopicsDataAccess();
			return da.GetByForum(forumId, startIndex, length);
		}

		public static Topic Get(int topicId)
		{
			TopicsDataAccess da = new TopicsDataAccess();
			return da.Get(topicId);
		}

		public static void LoadRelatedTopics(Topic topic, int amount)
		{
			TopicsDataAccess da = new TopicsDataAccess();
			topic.Related = da.GetRelatedTopics(topic, amount);
		}

		public static void AddReply(Message message, string ip)
		{
			message.ValidateFields();
			MessagesServiceClient.Add(message, ip);
		}

		public static void Create(Topic topic, string ip)
		{
			topic.ValidateFields();
			TopicsDataAccess da = new TopicsDataAccess();
			da.Add(topic, ip);
		}

		public static void Edit(Topic topic, string ip)
		{
			topic.ValidateFields();
			TopicsDataAccess da = new TopicsDataAccess();
			da.Edit(topic, ip);
		}

		public static void AddVisit(int topicId)
		{
			Action<int> handler = new Action<int>(AddVisitSync);
			handler.BeginInvoke(topicId, null, null);
		}

		private static void AddVisitSync(int topicId)
		{
			TopicsDataAccess da = new TopicsDataAccess();
			da.AddVisit(topicId);
		}

		public static List<Topic> GetLatest(int forumId, int startIndex, int length)
		{
			TopicsDataAccess da = new TopicsDataAccess();
			return da.GetByForumLatest(forumId, startIndex, length);
		}

		public static List<Topic> GetLatest()
		{
			TopicsDataAccess da = new TopicsDataAccess();
			return da.GetLatest();
		}
	}
}
