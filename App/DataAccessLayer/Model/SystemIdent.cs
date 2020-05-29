namespace Intersoft.CISSA.DataAccessLayer.Model
{
    public enum SystemIdent
    {
        Id,       // &Id - Id документа
        State,    // &State - Id текущего статуса документа
        Created,  // &Created - дата создания документа
        OrgId,    // &OrgId - Id организации-создателя документа
        OrgName,  // &OrgName - Наименование организации-создателя документа
        OrgCode,  // &OrgCode - Код организации-создателя документа
        UserId,   // &UserId - Код пользователя, создавшего документ
        UserName, // &UserName  - Имя пользователя, создавшего документ
        Modified, // &Modified - Дата последней модификации документа
        DefId,    // &DefId - Id класса документа
        InState,  // &InState - все статусы документа. Может использоваться только в условиях выборки. 
                  // Означает условие нахождения документа в указанном статусе!
        StateDate // Дата установления текущего статуса документа
    }
}