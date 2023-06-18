// ------------------------------------------------------------------------------
// <auto-generated>
//    Generated by avrogen, version 1.11.1
//    Changes to this file may cause incorrect behavior and will be lost if code
//    is regenerated
// </auto-generated>
// ------------------------------------------------------------------------------
namespace CrowdParlay.Users.Infrastructure.Communication.Messages
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using global::Avro;
	using global::Avro.Specific;
	
	[global::System.CodeDom.Compiler.GeneratedCodeAttribute("avrogen", "1.11.1")]
	public partial class UserUpdatedEvent : global::Avro.Specific.ISpecificRecord
	{
		public static global::Avro.Schema _SCHEMA = global::Avro.Schema.Parse("{\"type\":\"record\",\"name\":\"UserUpdatedEvent\",\"namespace\":\"CrowdParlay.Users.Infrast" +
				"ructure.Communication.Messages\",\"fields\":[{\"name\":\"UserId\",\"type\":\"string\"},{\"na" +
				"me\":\"Username\",\"type\":\"string\"},{\"name\":\"DisplayName\",\"type\":\"string\"}]}");
		private string _UserId;
		private string _Username;
		private string _DisplayName;
		public virtual global::Avro.Schema Schema
		{
			get
			{
				return UserUpdatedEvent._SCHEMA;
			}
		}
		public string UserId
		{
			get
			{
				return this._UserId;
			}
			set
			{
				this._UserId = value;
			}
		}
		public string Username
		{
			get
			{
				return this._Username;
			}
			set
			{
				this._Username = value;
			}
		}
		public string DisplayName
		{
			get
			{
				return this._DisplayName;
			}
			set
			{
				this._DisplayName = value;
			}
		}
		public virtual object Get(int fieldPos)
		{
			switch (fieldPos)
			{
			case 0: return this.UserId;
			case 1: return this.Username;
			case 2: return this.DisplayName;
			default: throw new global::Avro.AvroRuntimeException("Bad index " + fieldPos + " in Get()");
			};
		}
		public virtual void Put(int fieldPos, object fieldValue)
		{
			switch (fieldPos)
			{
			case 0: this.UserId = (System.String)fieldValue; break;
			case 1: this.Username = (System.String)fieldValue; break;
			case 2: this.DisplayName = (System.String)fieldValue; break;
			default: throw new global::Avro.AvroRuntimeException("Bad index " + fieldPos + " in Put()");
			};
		}
	}
}
