create or replace package UPGRADE_EXP_PREP is

  -- Author  : Doug Durham
  -- Created : 3/25/2014 9:22:30 AM
  -- Purpose : Utility package for preparing an upgrade export
  
  procedure add_upgrade_command
    (i_upg_id          integer,
    i_execution_order  integer,
    i_final_ind        char,
    i_retry_type       integer,
    i_section          integer,
    i_preview          varchar2,
    i_upg_command      clob,
    i_success_ind      char);

end UPGRADE_EXP_PREP;
/
create or replace package body UPGRADE_EXP_PREP is

  procedure add_upgrade_command
    (i_upg_id          integer,
    i_execution_order  integer,
    i_final_ind        char,
    i_retry_type       integer,
    i_section          integer,
    i_preview          varchar2,
    i_upg_command      clob,
    i_success_ind      char)
    is
    begin
          insert into rt_upgrade_command (upg_id,execution_order,final_ind,retry_type,section,preview,upg_command,success_ind)
          values (i_upg_id,
                 i_execution_order,
                 i_final_ind,
                 i_retry_type,
                 i_section,
                 i_preview,
                 i_upg_command,
                 i_success_ind);
    end;
    
end UPGRADE_EXP_PREP;
/
