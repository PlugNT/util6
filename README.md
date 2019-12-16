
![Util6 ORM](http://www.util6.com/content/website/images/logo.png "Util6 ORM")
## Util6 ORM 创建于2014年，于2019年正式开源，是在Util.Database数据库框架依赖的基础上实现的一个轻量级强类型ORM框架。
> 官方网站：https://www.util6.com
> 测试示例：<a href="https://www.cnblogs.com/dreamman/p/10805041.html" target="_blank">https://www.cnblogs.com/dreamman/p/10805041.html</a>

## Util6 ORM 特点

* 上手容易，语法简单，性能优，体积小
* 核心方法：查询(Query) 、新增(Insert)、更新(Update)、删除(Delete)
* 支持多数据库Sql Server,MySql,Access,等数据库
* 支持数据库读写分离配置、仓储操作、日志过滤、强类型约束、充血贫血模式


 
### 新增操作 
<pre class="prettyprint lang-cs">
public InfoResult< string > Insert(MisProjectEntity project)
{
    var result = new InfoResult< string > { IsSuccess = false };
    if (storage.MisProject.Query(m => m.Name == project.Name || m.ID == project.ID).ToCount() > 0)
    {
        return result.SetMessage("添加项目失败，已存在相同的项目！");
    }

    project.Remark = project.Remark ?? string.Empty;
    project.DatCreate = DateTime.Now;
    project.UpdateFlag = UtilityHelper.GetGuidForLong().ToString();
    project.AllowUserTypes = project.AllowUserTypes ?? string.Empty;
    project.AreaName = project.AreaName ?? string.Empty;

    var pid = storage.MisProject.SetEntity(project).Insert();
    if (pid <= 0) return result.SetMessage("添加项目失败！");
    CacheManageCore.RemoveProjects();

    var logInfo = string.Format("添加项目为{0}", project.Name);
    WriteDbLogInfo(LogOperType.新增, logInfo);
    return result.SetMessage($"添加项目成功！").SetSuccess();
}
</pre>



### 更新操作 
<pre class="prettyprint lang-cs">
public InfoResult< string > Update(MisUserEntity user)
{
    var result = new InfoResult< string > { IsSuccess = false };
    var entity = storage.MisUser.Query(m => m.ID == user.ID).ToEntity();
    if (entity == null)
    {
        return result.SetMessage("ID为" + user.ID + "的用户不存在！");
    }
    if (!string.IsNullOrWhiteSpace(user.Password))
    {
        entity.Password = entity.SaltPassword(user.Password);
        if (user.Password.Length < 6 || user.Password.Length > 20)
        {
            return result.SetMessage("修改用户失败，密码长度必须在6-20范围内！");
        }
    }
    entity.NickName = user.NickName ?? string.Empty;
    entity.IsEnabled = user.IsEnabled;

    var flag = user.UpdateFlag ?? string.Empty;
    entity.UpdateFlag = UtilityHelper.GetGuidForLong().ToString();
    //修改包括扩展用户
    if (!UserExtendCore.UpdateUser(entity, flag))
    {
        return result.SetMessage("修改用户失败，该记录可能在其他地方修改过！");
    }
    CacheManageCore.RemoveUser(entity.ID);
    CacheManageCore.RemoveRightsInfo(entity.ID);

    var logInfo = $"修改用户为{user.UserName}";
    WriteDbLogInfo(LogOperType.更新, logInfo);
    return result.SetMessage($"修改用户成功！").SetSuccess();
}
</pre>

 
### 删除操作 
<pre class="prettyprint lang-cs">
public InfoResult< string > Delete(int id)
{
    var result = new InfoResult< string > { IsSuccess = false };
    if (!storage.MisProject.Delete(m=>m.ID == id)) return result.SetMessage("删除项目失败！");
    CacheManageCore.RemoveProjects();

    var logInfo = string.Format("删除项目ID为{0}", id);
    WriteDbLogInfo(LogOperType.删除, logInfo);
    return result.SetMessage($"删除项目成功！").SetSuccess();            
}
</pre>

 
### 分页条件查询 
<pre class="prettyprint lang-cs">public FindPageListOutput FindPageList(FindPageListInput input)
{
    //查找日期
    var mapper = storage.MisLog.Query();
    if (!string.IsNullOrWhiteSpace(input.DatBegin))
    {
        var datBegin = DateTime.Parse(input.DatBegin);
        mapper.And(m => m.DatCreate >= datBegin);
    }
    if (!string.IsNullOrWhiteSpace(input.DatEnd))
    {
        var datEnd = DateTime.Parse(input.DatEnd);
        mapper.And(m => m.DatCreate <= datEnd);
    }
    //查找操作员
    if (!string.IsNullOrWhiteSpace(input.Oper))
    {
        mapper.And(m => m.Oper == input.Oper);
    }
    //分页及排序
    if (!string.IsNullOrWhiteSpace(input.SortName))
    {
        if (input.SortType == 0)
        {
            mapper.SortAsc(input.SortName);
        }
        else
        {
            mapper.SortDesc(input.SortName);
        }
    }

    var output = new FindPageListOutput();
    output.RecordCount = mapper.ToCount();
    output.LogList = mapper.ToList(input.PageSize, input.CurrPage, output.RecordCount);
    return output;
}</pre>

 
### 多表查询 
<pre class="prettyprint lang-cs">private MisDataAccessStorage storage = new MisDataAccessStorage();
public FindMaintainRoleOutput FindMaintainRole(int roleID)
{
    var output = new FindMaintainRoleOutput();
    output.Role = storage.MisRole.Query(m => m.ID == roleID).ToEntity();
    if (output.Role != null)
    {
        output.Project = storage.MisProject.Query(m => m.ID == output.Role.ProjectID).ToEntity();
        output.UserRoleList = storage.MisUserRole.Query(m => m.RoleID == output.Role.ID).ToList();
        output.RoleRightsList = storage.MisRoleRights.Query(m => m.RoleID == output.Role.ID).ToList();
        output.RoleExtendList = storage.MisRoleExtend.Query(m => m.RoleID == output.Role.ID).ToList();
        var rightsIDList = output.RoleRights.Select(m => m.RightsID).ToList();
        if (rightsIDList.Count > 0)
        {
            output.RightsList = storage.MisRights.Query(m=> rightsIDList.Contains(m.ID)).ToList();
            output.RightsPageList = storage.MisRightsPage.Query(m => rightsIDList.Contains(m.RightsID)).ToList();
        }
    }
    return output;
}</pre>

 
### 扩展关联 
<pre class="prettyprint lang-cs">public List< MisRightsPage > FindRightsPageSet(int userID,int projectID)
{
    var config = DbReadConfig;
    var strSql = $@"SELECT {AllFields} FROM [{T.MisRightsPage}] WHERE {T.MisRightsPage_ProjectID}=@ProjectID and {T.MisRightsPage_RightsID} IN 
    (
        SELECT {T.MisRoleRights_RightsID} FROM [{T.MisRoleRights}] WHERE {T.MisRoleRights_RoleID} IN 
        (
            SELECT a.{T.MisUserRole_RoleID} FROM [{T.MisUserRole}] as a inner join [{T.MisRole}] as b on a.{T.MisUserRole_RoleID}=b.{T.MisRole_ID} 
            WHERE a.{T.MisUserRole_UserID}=@UserID and b.{T.MisRole_IsEnabled} = 1
        )
    )";
    var paramters = new { ProjectID = projectID, UserID = userID };
    return GetList(new DbBuilder(config).GetDataReader(strSql, paramters));
}</pre>

 
### 批量操作
<pre class="prettyprint lang-cs">public void Execute(TicketOrder order)
{
    var adultPolicy = storage.PmsPolicyTemplate.Query(m =&gt; m.ID == order.AdultPolicyID).ToEntity();
    if (adultPolicy != null)
    {
        //只修改字段SellCabinCount
        storage.PmsPolicyTemplate.SetEntity(adultPolicy).SetPartHandled();
        adultPolicy.SellCabinCount = adultPolicy.SellCabinCount - travelerInfo.AdultCount;
        storage.PmsPolicyTemplate.`AttachUpdate`(m =&gt; m.ID == order.AdultPolicyID);
    }
    if (!string.IsNullOrWhiteSpace(order.ChildPolicyID))
    {
        var childPolicy = storage.PmsPolicyTemplate.Query(m =&gt; m.ID == order.ChildPolicyID).ToEntity();
        if (childPolicy != null)
        {
            storage.PmsPolicyTemplate.SetEntity(childPolicy).SetPartHandled();
            childPolicy.SellCabinCount = childPolicy.SellCabinCount - (travelerInfo.ChildCount + travelerInfo.InfantCount);
            storage.PmsPolicyTemplate.`AttachUpdate`(m =&gt; m.ID == order.ChildPolicyID);
        }
    }
    storage.`SaveChanges`();
    return output;
}
</pre>
