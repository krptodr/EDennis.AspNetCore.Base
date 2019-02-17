﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Data;

namespace EDennis.AspNetCore.Base.Testing {
    public class CloneClassFixture : ConfigurationClassFixture, IDisposable {        

        public BlockingCollection<int> CloneIndexPool { get; } 
            = new BlockingCollection<int>();

        public CloneConnections CloneConnections { get; }

        public const int DEFAULT_CLONE_COUNT = 5;


        public CloneClassFixture() : base() {
            var cloneCountStr = Configuration["Testing:DatabaseCloneCount"] ?? DEFAULT_CLONE_COUNT.ToString() ;
            var cloneCount = int.Parse(cloneCountStr);

            CloneConnections = DatabaseCloneManager.GetCloneConnections(Configuration, cloneCount);
            CloneConnections.AutomatedTest = true;

            for (int i=0; i<cloneCount; i++) {
                CloneIndexPool.Add(i);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected override void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    foreach (var context in CloneConnections.Keys) {
                        foreach (var cxn in CloneConnections[context]) {
                            if (cxn.SqlConnection.State == ConnectionState.Open) {
                                cxn.SqlConnection.Close();
                            }
                        }
                    }

                }
                disposedValue = true;
            }
        }
        #endregion


    }
}