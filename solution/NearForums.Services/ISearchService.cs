﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NearForums.Services
{
	public interface ISearchService
	{
		/// <summary>
		/// Creates or recreates the index.
		/// Delete all previous index data.
		/// </summary>
		void CreateIndex();

		/// <summary>
		/// Adds a message to the index
		/// </summary>
		/// <param name="topic"></param>
		void Add(Message message);

		/// <summary>
		/// Adds a topic to the index
		/// </summary>
		void Add(Topic topic);

		/// <summary>
		/// Removes a message from the search index
		/// </summary>
		/// <param name="message"></param>
		void DeleteMessage(int topicId, int messageId);

		/// <summary>
		/// Removes a topic from the search index
		/// </summary>
		/// <param name="topic"></param>
		void Delete(Topic topic);

		/// <summary>
		/// Queries the index
		/// </summary>
		List<Topic> Search(string query);

		/// <summary>
		/// Updates a topic from the 
		/// </summary>
		/// <param name="topic"></param>
		void Update(Topic topic);
	}
}