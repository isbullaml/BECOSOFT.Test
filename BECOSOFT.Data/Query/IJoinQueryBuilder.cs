using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Services.Interfaces;
using System.Collections.Generic;

namespace BECOSOFT.Data.Query {
    internal interface IJoinQueryBuilder : IBaseService {

        /// <summary>
        /// Loops the entities tree to find all linked entities and linked entity and adds them to a list of prepared joins
        /// </summary>
        /// <param name="linkedEntitiesTree">Tree to loop</param>
        /// <param name="includeLinkedEntities">Include linked entities</param>
        /// <param name="includeLinkedEntity">Include linked entity</param>
        /// <param name="tablePart"></param>
        List<string> AddLinkedEntitiesJoinParts(ITreeNode<EntityTreeNode> linkedEntitiesTree,
                                                bool includeLinkedEntities, bool includeLinkedEntity, string tablePart);
    }
}