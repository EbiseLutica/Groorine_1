using System;
using System.Windows.Input;

namespace Groorine
{
	/// <summary> 
	/// デリゲートを受け取るICommandの実装 
	/// </summary> 
	public class DelegateCommand : ICommand
	{
		private readonly Action<object> _execute;

		private readonly Func<bool> _canExecute;

		/// <summary> 
		/// コマンドのExecuteメソッドで実行する処理を指定してDelegateCommandのインスタンスを 
		/// 作成します。 
		/// </summary> 
		/// <param name="execute">Executeメソッドで実行する処理</param> 
		public DelegateCommand(Action<object> execute) : this(execute, () => true)
		{
		}

		/// <summary> 
		/// コマンドのExecuteメソッドで実行する処理とCanExecuteメソッドで実行する処理を指定して 
		/// DelegateCommandのインスタンスを作成します。 
		/// </summary> 
		/// <param name="execute">Executeメソッドで実行する処理</param> 
		/// <param name="canExecute">CanExecuteメソッドで実行する処理</param> 
		public DelegateCommand(Action<object> execute, Func<bool> canExecute)
		{
			if (execute == null)
			{
				throw new ArgumentNullException(nameof(execute));
			}

			if (canExecute == null)
			{
				throw new ArgumentNullException(nameof(canExecute));
			}

			_execute = execute;
			_canExecute = canExecute;
		}
		

		/// <summary> 
		/// コマンドが実行可能な状態化どうか問い合わせます。 
		/// </summary> 
		/// <returns>実行可能な場合はtrue</returns> 
		public bool CanExecute()
		{
			return _canExecute();
		}

		/// <summary> 
		/// ICommand.CanExecuteの明示的な実装。CanExecuteメソッドに処理を委譲する。 
		/// </summary> 
		/// <param name="parameter"></param> 
		/// <returns></returns> 
		bool ICommand.CanExecute(object parameter)
		{
			return CanExecute();
		}

		/// <summary> 
		/// ICommand.Executeの明示的な実装。Executeメソッドに処理を委譲する。 
		/// </summary> 
		/// <param name="parameter"></param> 
		void ICommand.Execute(object parameter)
		{
			_execute(parameter);
		}

		public event EventHandler CanExecuteChanged;
	}
}