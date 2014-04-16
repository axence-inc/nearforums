using NearForums.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NearForums.Tests.TestCleanup
{
    public abstract partial class Cleaner
    {
        public abstract void AddTestObject(object obj);
        public abstract void Cleanup();
        public abstract bool Cleanup(object obj);

        private static Cleaner _instance;

        public static Cleaner Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CleanerComposite(
                        new UserCleaner(),
                        new ForumCategoryCleaner(), 
                        new ForumCleaner(),
                        new TopicCleaner());
                }

                return _instance;
            }
        }

        protected abstract Type GetModelType();

        private class CleanerComposite : Cleaner
        {
            private IDictionary<Type, Cleaner> _cleaners;

            public CleanerComposite(params Cleaner[] cleaners)
            {
                _cleaners = cleaners
                    .AsEnumerable<Cleaner>()
                    .ToDictionary<Cleaner, Type>(cln => cln.GetModelType());
            }

            public override void AddTestObject(object obj)
            {
                if (!_cleaners.ContainsKey(obj.GetType()))
                {
                    throw new ArgumentException("obj type not expected : " +obj.GetType().FullName);
                }

                Cleaner cleaner = _cleaners[obj.GetType()];
                cleaner.AddTestObject(obj);
            }

            public override void Cleanup()
            {
                foreach (Cleaner cleaner in _cleaners.Values.Reverse())
                {
                    cleaner.Cleanup();
                }
            }

            public override bool Cleanup(object obj)
            {
                Cleaner cleaner;

                if (!_cleaners.ContainsKey(obj.GetType()))
                {
                    throw new NotImplementedException(
                        string.Format(
                            "Cleaner for object of type '{0}' not implemented", obj.GetType().FullName));
                }
                
                cleaner = _cleaners[obj.GetType()];
                return cleaner.Cleanup(obj);
            }

            protected override Type GetModelType()
            {
                throw new NotImplementedException();
            }
        }

        private abstract class CleanerGeneric<TModel, TDataAccess, TKey> : Cleaner
            where TDataAccess : DataAccess.BaseDataAccess, new()
        {
            private IList<TModel> _artifacts;

            public override void AddTestObject(object obj)
            {
                if (!(obj is TModel))
                {
                    throw new ArgumentException(
                        string.Format(
                            "Argument of {0} type expected", typeof(TModel).Name));
                }

                this.AddTestObject((TModel)obj);
            }

            public void AddTestObject(TModel obj)
            {
                if (_artifacts == null)
                {
                    _artifacts = new List<TModel>();
                }

                _artifacts.Add(obj);
            }

            public override void Cleanup()
            {
                if (_artifacts != null)
                {
                    foreach (TModel obj in _artifacts.Reverse())
                    {
                        this.Cleanup(obj);
                    }
                }
            }

            public override bool Cleanup(object o)
            {
                return this.Cleanup((TModel)o);
            }

            public bool Cleanup(TModel obj)
            {
                ICleanerDataAccess<TModel> cda = this.GetDataAccessObject();
                return cda.PermanentlyDelete(obj);
            }

            protected abstract ICleanerDataAccess<TModel> GetDataAccessObject();

            protected override Type GetModelType()
            {
                return typeof(TModel);
            }
        }

        private class UserCleaner : CleanerGeneric<User, DataAccess.UsersDataAccess, string>
        {
            protected override ICleanerDataAccess<User> GetDataAccessObject()
            {
                return new UsersDataAccess();
            }

            private class UsersDataAccess : BaseCleanerDataAccess<User, DataAccess.UsersDataAccess, int>
            {
                protected override string TableName  { get { return "Users"; } }

                protected override string KeyColumnName { get { return "userid"; } }

                protected override int GetKeyValue(User obj)
                {
                    return obj.Id;
                }

                public override bool PermanentlyDelete(User obj)
                {
                    System.Web.Security.Membership.DeleteUser(obj.UserName);
                    
                    return base.PermanentlyDelete(obj);
                }
            }
        }

        private class ForumCleaner : CleanerGeneric<Forum, DataAccess.ForumsDataAccess, string>
        {
            protected override ICleanerDataAccess<Forum> GetDataAccessObject()
            {
                return new ForumDataAccess();
            }

            private class ForumDataAccess : BaseCleanerDataAccess<Forum, DataAccess.ForumsDataAccess, string>
            {
                protected override string TableName { get { return "Forums"; } }
                
                protected override string KeyColumnName { get { return "forumshortname"; } }

                protected override string GetKeyValue(Forum obj)
                {
                    return obj.ShortName;
                }
            }
        }

        private class ForumCategoryCleaner : CleanerGeneric<ForumCategory, DataAccess.ForumCategoriesDataAccess, int>
        {
            protected override ICleanerDataAccess<ForumCategory> GetDataAccessObject()
            {
                return new ForumCategoryDataAccess();
            }

            private class ForumCategoryDataAccess : BaseCleanerDataAccess<ForumCategory, DataAccess.ForumCategoriesDataAccess, int>
            {
                protected override string TableName { get { return "forumscategories"; } }
                
                protected override string KeyColumnName { get { return "categoryid"; } }

                protected override int GetKeyValue(ForumCategory obj)
                {
                    return obj.Id;
                }
            }
        }

        private class TopicCleaner : CleanerGeneric<Topic, DataAccess.TopicsDataAccess, int>
        {
            protected override ICleanerDataAccess<Topic> GetDataAccessObject()
            {
                return new TopicDataAccess();
            }

            private class TopicDataAccess : BaseCleanerDataAccess<Topic, DataAccess.TopicsDataAccess, int>
            {
                protected override string TableName { get { return "topics"; } }
                protected override string KeyColumnName { get { return "topicid"; } }

                protected override int GetKeyValue(Topic obj)
                {
                    return obj.Id;
                }

                public override bool PermanentlyDelete(Topic obj)
                {
                    DataAccess.BaseDataAccess bda = new DataAccess.TopicsDataAccess();

                    AssertNoErrors(
                        base.GetDeletePermanentlyCommand(bda, "topicssubscriptions", "topicid", obj.Id)
                            .SafeExecuteAndGetNoOfRowsAffected());

                    AssertNoErrors(
                        base.GetDeletePermanentlyCommand(bda, "tags", "topicid", obj.Id)
                            .SafeExecuteAndGetNoOfRowsAffected());
                    AssertNoErrors(
                        base.GetDeletePermanentlyCommand(bda, "messages", "topicid", obj.Id)
                            .SafeExecuteAndGetNoOfRowsAffected());

                    return base.PermanentlyDelete(bda, obj);
                }
                
                private void AssertNoErrors(int resultCode)
                {
                    if (0 > resultCode)
                    {
                        throw new Exception("Error while excecuting command");
                    }
                }
            }
        }
    }
}
