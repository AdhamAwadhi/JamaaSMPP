/************************************************************************
 * Copyright (C) 2007 Jamaa Technologies
 *
 * This file is part of Jamaa SMPP Library.
 *
 * Jamaa SMPP Library is free software. You can redistribute it and/or modify
 * it under the terms of the Microsoft Reciprocal License (Ms-RL)
 *
 * You should have received a copy of the Microsoft Reciprocal License
 * along with Jamaa SMPP Library; See License.txt for more details.
 *
 * Author: Benedict J. Tesha
 * benedict.tesha@jamaatech.com, www.jamaatech.com
 *
 ************************************************************************/

using System.Threading;

namespace JamaaTech.Smpp.Net.Lib.Util
{
    public abstract class RunningComponent
    {
        #region Variables
        protected bool vRunning;
        protected object vSyncRoot;
        protected Thread vRunningThread;
        private bool vStopOnNextCycle;
        #endregion

        #region Constructors
        public RunningComponent()
        {
            //Initit vSyncRoot
            vSyncRoot = new object();
            //vRunning = false; //false is the default boolean value anyway,  not need to set it
        }

        #endregion

        #region Properties
        public bool Running
        {
            get { lock (vSyncRoot) { return vRunning; } }
        }
        #endregion

        #region Methods
        #region Interface Methods
        public void Start()
        {
            lock (vSyncRoot) { if (vRunning) { return; } } //If this component is already running, do nothing
            //Initialize component before running owner thread
            InitializeComponent();
            RunThread();
        }

        public void Stop()
        {
            Stop(false);
        }

        public void Stop(bool allowCompleteCycle)
        {
            lock (vSyncRoot)
            {
                if (!vRunning) { return; } //If this component is stopped, do nothing
                vStopOnNextCycle = true; //Prevent running thread from continue looping
                if (!allowCompleteCycle)
                {
                    vRunningThread.Abort(); //Abort owner thread
                    vRunningThread.Join(); //Wait until thread abort is complete
                    vRunning = false;
                    vRunningThread = null;
                }
            }
        }

        protected abstract void RunNow();

        protected virtual void ThreadCallback()
        {
            lock (vSyncRoot) { vRunning = true; }
            RunNow();
            lock (vSyncRoot)
            {
                vRunning = false;
                vRunningThread = null;
            }
        }

        protected virtual void InitializeComponent() { }

        protected virtual bool CanContinue()
        {
            lock (vSyncRoot) { return !vStopOnNextCycle; }
        }

        protected virtual void StopOnNextCycle()
        {
            lock (vSyncRoot) { vStopOnNextCycle = true; }
        }
        #endregion

        #region Helper Methods
        private void RunThread()
        {
            vRunningThread = new Thread(new ThreadStart(ThreadCallback));
            //Make it a background thread so that it does not keep the
            //application running after the main threads exit
            vRunningThread.IsBackground = true;
            //Start the thread
            vRunningThread.Start();
        }
        #endregion
        #endregion
    }
}
