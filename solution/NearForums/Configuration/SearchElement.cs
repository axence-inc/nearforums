﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;

namespace NearForums.Configuration
{
	public class SearchElement : ConfigurationElement
	{
		public SiteConfiguration ParentElement
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the index file path.
		/// </summary>
		public string IndexPath
		{
			get
			{
				return Path.Combine(ParentElement.ContentPathFull, "search-index");
			}
		}

		/// <summary>
		/// Determines the max amount of messages to be indexed per topic
		/// </summary>
		[ConfigurationProperty("maxMessages", IsRequired = false, DefaultValue = 20)]
		public int MaxMessages
		{
			get
			{
				return (int)this["maxMessages"];
			}
			set
			{
				this["maxMessages"] = value;
			}
		}

		/// <summary>
		/// Determines the max amount of results on a search query
		/// </summary>
		[ConfigurationProperty("maxResults", IsRequired = false, DefaultValue = 200)]
		public int MaxResults
		{
			get
			{
				return (int)this["maxResults"];
			}
			set
			{
				this["maxResults"] = value;
			}
		}

		/// <summary>
		///  Gets or sets the boost factor hits on the topic title field.
		///  This value will be multiplied into the score of all hits on this this field of this document.
		/// </summary>
		[ConfigurationProperty("titleBoost", IsRequired = false, DefaultValue = 2f)]
		public float TitleBoost
		{
			get
			{
				return (float)this["titleBoost"];
			}
			set
			{
				this["titleBoost"] = value;
			}
		}

		/// <summary>
		///  Gets or sets the boost factor hits on the topic description field.
		///  This value will be multiplied into the score of all hits on this this field of this document.
		/// </summary>
		[ConfigurationProperty("descriptionBoost", IsRequired = false, DefaultValue = 1.5f)]
		public float DescriptionBoost
		{
			get
			{
				return (float)this["descriptionBoost"];
			}
			set
			{
				this["descriptionBoost"] = value;
			}
		}

		/// <summary>
		///  Gets or sets the boost factor hits on the topic tags field.
		///  This value will be multiplied into the score of all hits on this this field of this document.
		/// </summary>
		[ConfigurationProperty("tagsBoost", IsRequired = false, DefaultValue = 3f)]
		public float TagsBoost
		{
			get
			{
				return (float)this["tagsBoost"];
			}
			set
			{
				this["tagsBoost"] = value;
			}
		}
	}
}