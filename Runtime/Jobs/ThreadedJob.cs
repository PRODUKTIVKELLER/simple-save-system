using System.Collections;
using System.Threading;

namespace Produktivkeller.SimpleSaveSystem.Jobs
{
    public class ThreadedJob
    {
        private bool   _isDone;
        private object _handle = new object();
        private Thread _thread = null;

        public bool IsDone
        {
            get
            {
                bool tmp;
                lock (_handle)
                {
                    tmp = _isDone;
                }
                return tmp;
            }
            set
            {
                lock (_handle)
                {
                    _isDone = value;
                }
            }
        }

        public virtual void Start()
        {
            _thread = new System.Threading.Thread(Run);
            _thread.Start();
        }

        public virtual void Abort()
        {
            _thread.Abort();
        }

        protected virtual void ThreadFunction() { }

        protected virtual void OnFinished() { }

        public virtual bool Update()
        {
            if (IsDone)
            {
                OnFinished();
                return true;
            }
            return false;
        }

        IEnumerator WaitFor()
        {
            while (!Update())
            {
                yield return null;
            }
        }

        private void Run()
        {
            ThreadFunction();

            IsDone = true;
        }

        public void Join()
        {
            if (_thread != null)
            {
                _thread.Join();
            }
        }
    }
}