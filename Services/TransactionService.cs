using GeodicBankAPI.Application.Interfaces;
using GeodicBankAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeodicBankAPI.Application.Services
{
    public class TransactionService : ITransaction
    {
        public Transaction GetTransaction(string Id)
        {
            var response =  new Transaction();

            return  response;
        }

        public Transaction RecieveTransaction(string Id)
        {
            var response = new Transaction();

            return response;
        }
    }
}
