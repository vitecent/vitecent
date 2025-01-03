﻿/*
 *
 * 版权所有 ：易鹏航服
 * 作   者 : duhuifeng
 *
 */

namespace YPHF.Core.Orm.SqlSugar;

/// <summary>
/// Class SqlSugarTable. Implements the <see cref="YPHF.Core.Orm.BaseTable" />
/// </summary>
/// <seealso cref="YPHF.Core.Orm.BaseTable" />
public class SqlSugarTable : BaseTable
{
    /// <summary>
    /// Gets or sets the fields.
    /// </summary>
    /// <value>The fields.</value>
    public List<SqlSugarField> Fields { get; set; } = default!;
}