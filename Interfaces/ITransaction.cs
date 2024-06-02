using GeodicBankAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeodicBankAPI.Application.Interfaces
{
    public interface ITransaction
    {
        public Transaction GetTransaction(string Id);
        public Transaction RecieveTransaction(string Id);
    }
}
