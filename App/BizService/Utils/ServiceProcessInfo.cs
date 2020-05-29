using System;

namespace Intersoft.CISSA.BizService.Utils
{
    public class ServiceProcessInfo
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public Guid UserId { get; set; }

        public DateTime StartTime { get; set; }
    }
}