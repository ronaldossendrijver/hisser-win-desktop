/*
 * Copyright (C) 2014, by Ronald Ossendrijver <ronaldossendrijver@gmail.com>.
 */
using System;
using System.Linq;
using System.Data;
using System.Data.Linq;
using System.Data.Common;

namespace Hisser.ClientServer
{
    /// <summary>
    /// Stores and retrieves Contact information in a Data Store. Contacts retrieved from this Manager
    /// can be edited within a Transaction. Aborted transactions will roll back any changes made, commited
    /// transactions will make sure the changes are persisted.
    /// </summary>
    public class ContactManager
    {
        private ContactContext Context { get; set; }
        private string _dcConnextionString;

        /// <summary>
        /// Creates a new Contect Manager.
        /// </summary>
        /// <param name="dcConnectionString">The connection string of the database to use for storage. 
        /// Example: "DataSource=isostore:/database.sdf" specifies a SQL Server Compact database in Phone storage.</param>
        public ContactManager(string dcConnectionString)
        {
            _dcConnextionString = dcConnectionString;
            ResetContext();
        }

        /// <summary>
        /// Begins a Transaction in which changes can be made to a Contacts and related Entities.
        /// </summary>
        /// <returns>The started Transaction. This Transaction should either be Commit()-ed or RollBack()-ed.</returns>
        public ContactTransaction BeginTransaction()
        {
            return new ContactTransaction(this);
        }

        /// <summary>
        /// Represents all Contacts managed by this ContactManager.
        /// </summary>
        public Table<Contact> Contacts
        {
            get
            {
                return Context.Contacts;
            }
        }
        
        /// <summary>
        /// Disposes the current ContactContext, discarding all pending changes since the last Commit(). Entities
        /// will be freshly loaded from the database.
        /// </summary>
        private void ResetContext()
        {
            if (Context != null)
            {
                Context.Dispose();
            }

            Context = new ContactContext(_dcConnextionString);

            //Create a database to persist the context in case it doesn't exist yet.
            if (!Context.DatabaseExists())
            {
                try
                {
                    Context.CreateDatabase();
                }
                catch (Exception)
                {
                    throw new DatabaseCreationException();
                }
            }
            else
            {
                try
                {
                    //This "ugly" piece of code is necessary to make sure that the existing database is correct.
                    int nrOfContacts = Context.Contacts.Count();
                }
                catch (Exception)
                {
                    Context.DeleteDatabase();
                    Context.CreateDatabase();
                }
            }
        }

        /// <summary>
        /// Resembles a database transaction in which Contact information is altered and persisted.
        /// </summary>
        public class ContactTransaction : IDisposable
        {
            internal ContactManager _manager;
            internal bool completed = false;

            internal ContactTransaction(ContactManager manager)
            {
                this._manager = manager;
            }

            /// <summary>
            /// Commits the transaction, persisting all pending changes.
            /// </summary>
            public void Commit()
            {
                _manager.Context.SubmitChanges();
                completed = true;
            }

            /// <summary>
            /// Rolls back the transaction, discarding all pending changes.
            /// </summary>
            public void Rollback()
            {
                _manager.ResetContext();
                completed = true;
            }

            /// <summary>
            /// Disposes of this transaction, rolling back all changes if they have not been committed.
            /// </summary>
            public void Dispose()
            {
                if (!completed)
                {
                    Rollback();
                }
            }
        }

        /// <summary>
        /// Resembles a DataContext to manipulate Contact information.
        /// </summary>
        private class ContactContext : DataContext
        {
            public Table<Contact> Contacts = null;
            public Table<SentComponent> SentComponents = null;
            public Table<ReceivedComponent> ReceivedComponents = null;
            public Table<MessageData> Messages = null;

            public ContactContext(string dcConnectionString)
                : base(dcConnectionString)
            {

            }
        }    
    }

}
