﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NearForums.DataAccess
{
    public interface IForumCategoriesDataAccess
    {
        List<ForumCategory> GetAll();
        ForumCategory Get(int id);
        void Add(ForumCategory category);
        void Edit(ForumCategory category);
        bool Delete(int id);
        int GetForumCount(int id);
        
    }
}
