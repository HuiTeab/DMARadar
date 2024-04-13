namespace DMARadar.Components.Services
{
    public class GameLoopService
    {
        private Timer? _timer;
        private bool _isLoopRunning = false;
        private int _loopDelay = 300;  // Default delay

        public bool IsLoopRunning => _isLoopRunning;
        public int LoopDelay
        {
            get => _loopDelay;
            set
            {
                if (_loopDelay != value)
                {
                    _loopDelay = value;
                    if (_isLoopRunning && _timer != null)
                    {
                        _timer.Change(0, _loopDelay);  // Update existing timer with new delay
                    }
                }
            }
        }

        public void StartLoop(Action loopAction)
        {
            if (!_isLoopRunning)
            {
                _timer = new Timer(_ => loopAction(), null, 0, _loopDelay);
                _isLoopRunning = true;
            }
        }

        public void StopLoop()
        {
            if (_isLoopRunning && _timer != null)
            {
                _timer.Change(Timeout.Infinite, 0);
                _isLoopRunning = false;
            }
        }
    }


}
