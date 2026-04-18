using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Models.QueryData;
using BECOSOFT.Data.Repositories.Interfaces.QueryData;
using BECOSOFT.Data.Services.Interfaces.QueryData;
using System;
using System.Collections.Generic;

namespace BECOSOFT.Data.Services.QueryData {
    public sealed class TriggerQueryService : ITriggerQueryService {
        private readonly ITriggerQueryRepository _repository;

        public TriggerQueryService(ITriggerQueryRepository repository) {
            _repository = repository;
        }

        public TriggerEnableResult EnableTrigger<T>(string triggerName, string tablePart = null) where T : IEntity {
            return _repository.EnableTrigger<T>(triggerName, tablePart);
        }

        public TriggerEnableResult EnableTrigger(Schema schema, string tableName, string triggerName, string tablePart = null) {
            return _repository.EnableTrigger(schema, tableName, triggerName, tablePart);
        }

        public TriggerDisableResult DisableTrigger<T>(string triggerName, string tablePart = null) where T : IEntity {
            return _repository.DisableTrigger<T>(triggerName, tablePart);
        }

        public TriggerDisableResult DisableTrigger(Schema schema, string tableName, string triggerName, string tablePart = null) {
            return _repository.DisableTrigger(schema, tableName, triggerName, tablePart);
        }

        public TriggerQueryResult GetTrigger<T>(string triggerName, string tablePart = null) where T : IEntity {
            return _repository.GetTrigger<T>(triggerName, tablePart);
        }

        public TriggerQueryResult GetTrigger(Schema schema, string tableName, string triggerName, string tablePart = null) {
            return _repository.GetTrigger(schema, tableName, triggerName, tablePart);
        }

        public List<TriggerQueryResult> GetTriggers<T>(string triggerName, string tablePart = null) where T : IEntity {
            return _repository.GetTriggers<T>(triggerName, tablePart);
        }

        public List<TriggerQueryResult> GetTriggers(Schema schema, string tableName, string triggerName, string tablePart = null) {
            return _repository.GetTriggers(schema, tableName, triggerName, tablePart);
        }

        public void DisableTemporarily<T>(Action action, string triggerName, string tablePart = null) where T : IEntity {
            var disableResult = DisableTrigger<T>(triggerName, tablePart);
            try {
                action();
            } finally {
                if (!disableResult.AlreadyDisabled) {
                    EnableTrigger<T>(triggerName, tablePart);
                }
            }
        }

        public void DisableTemporarily(Action action, Schema schema, string tableName, string triggerName, string tablePart = null) {
            var disableResult = DisableTrigger(schema, tableName, triggerName, tablePart);
            try {               
                action();
            } finally {
                if (!disableResult.AlreadyDisabled) {
                    EnableTrigger(schema, tableName, triggerName, tablePart);
                }
            }
        }
    }
}