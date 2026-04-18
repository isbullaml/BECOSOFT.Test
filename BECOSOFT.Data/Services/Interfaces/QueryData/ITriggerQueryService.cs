using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Models.QueryData;
using System;
using System.Collections.Generic;

namespace BECOSOFT.Data.Services.Interfaces.QueryData {
    public interface ITriggerQueryService : IBaseService {
        /// <summary>
        /// Enables trigger <see cref="triggerName"/> on the table defined by <see cref="T"/> (with optional <see cref="tablePart"/>).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="triggerName"></param>
        /// <param name="tablePart"></param>
        /// <exception cref="ArgumentException"><see cref="triggerName"/> does not exist on the table.</exception>
        /// <returns></returns>
        TriggerEnableResult EnableTrigger<T>(string triggerName, string tablePart = null) where T : IEntity;

        /// <summary>
        /// Enables trigger <see cref="triggerName"/> on the table defined by <see cref="schema"/>.<see cref="tableName"/> (with optional <see cref="tablePart"/>).
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="triggerName"></param>
        /// <param name="tablePart"></param>
        /// <param name="schema"></param>
        /// <exception cref="ArgumentException"><see cref="triggerName"/> does not exist on the table.</exception>
        /// <returns></returns>
        TriggerEnableResult EnableTrigger(Schema schema, string tableName, string triggerName, string tablePart = null);

        /// <summary>
        /// Disables trigger <see cref="triggerName"/> on the table defined by <see cref="T"/> (with optional <see cref="tablePart"/>).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="triggerName"></param>
        /// <param name="tablePart"></param>
        /// <exception cref="ArgumentException"><see cref="triggerName"/> does not exist on the table.</exception>
        /// <returns></returns>
        TriggerDisableResult DisableTrigger<T>(string triggerName, string tablePart = null) where T : IEntity;

        /// <summary>
        /// Disables trigger <see cref="triggerName"/> on the table defined by <see cref="schema"/>.<see cref="tableName"/> (with optional <see cref="tablePart"/>).
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="triggerName"></param>
        /// <param name="tablePart"></param>
        /// <param name="schema"></param>
        /// <exception cref="ArgumentException"><see cref="triggerName"/> does not exist on the table.</exception>
        /// <returns></returns>
        TriggerDisableResult DisableTrigger(Schema schema, string tableName, string triggerName, string tablePart = null);


        /// <summary>
        /// Get the information for trigger <see cref="triggerName"/> on the table defined by <see cref="T"/> (with optional <see cref="tablePart"/>).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="triggerName"></param>
        /// <param name="tablePart"></param>
        /// <returns></returns>
        TriggerQueryResult GetTrigger<T>(string triggerName, string tablePart = null) where T : IEntity;

        /// <summary>
        /// Get the information for trigger <see cref="triggerName"/> on the table defined by <see cref="schema"/>.<see cref="tableName"/> (with optional <see cref="tablePart"/>).
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="triggerName"></param>
        /// <param name="tablePart"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        TriggerQueryResult GetTrigger(Schema schema, string tableName, string triggerName, string tablePart = null);

        /// <summary>
        /// Lists all triggers on the table defined by <see cref="T"/> (with optional <see cref="tablePart"/>).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="triggerName"></param>
        /// <param name="tablePart"></param>
        /// <returns></returns>
        List<TriggerQueryResult> GetTriggers<T>(string triggerName, string tablePart = null) where T : IEntity;

        /// <summary>
        /// Lists all triggers on the table defined by <see cref="schema"/>.<see cref="tableName"/> (with optional <see cref="tablePart"/>).
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="triggerName"></param>
        /// <param name="tablePart"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        List<TriggerQueryResult> GetTriggers(Schema schema, string tableName, string triggerName, string tablePart = null);

        /// <summary>
        /// Performs <see cref="action"/> in bitween disabling and enabling  <see cref="triggerName"/> on the table defined by <see cref="T"/> (with optional <see cref="tablePart"/>).
        /// If the trigger is already disabled, it will not be enabled by the function.
        /// /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="triggerName"></param>
        /// <param name="tablePart"></param>
        void DisableTemporarily<T>(Action action, string triggerName, string tablePart = null) where T : IEntity;

        /// <summary>
        /// Performs <see cref="action"/> in bitween disabling and enabling <see cref="triggerName"/> on the table defined by <see cref="schema"/>.<see cref="tableName"/> (with optional <see cref="tablePart"/>).
        /// If the trigger is already disabled, it will not be enabled by the function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="schema"></param>
        /// <param name="tableName"></param>
        /// <param name="triggerName"></param>
        /// <param name="tablePart"></param>
        void DisableTemporarily(Action action, Schema schema, string tableName, string triggerName, string tablePart = null);
    }
}