using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Current_Cycling_Controls {
    public abstract class Recipe<TRecipeType> {
        public ObjectId Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public bool Selected { get; set; }
        public string Table { get; set; }
        protected Recipe() {
            Id = new ObjectId();
            Created = DateTime.UtcNow;
            Updated = DateTime.UtcNow;
            Active = true;
        }
        public override string ToString() {
            return Name;
        }
    }



}
