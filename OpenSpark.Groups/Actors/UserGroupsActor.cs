using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace OpenSpark.Groups.Actors
{
    public class UserGroupsActor : ReceiveActor
    {

        public UserGroupsActor()
        {
            Receive<UserGroupsActor>(query =>
            {



            });
        }
    }
}
