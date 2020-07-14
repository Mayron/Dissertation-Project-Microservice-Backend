using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands
{
    public class CreateUserPostRequestCommand : ICommand
    {
        public Post Post { get; set; }
        public string GroupId { get; set; }
    }
}
