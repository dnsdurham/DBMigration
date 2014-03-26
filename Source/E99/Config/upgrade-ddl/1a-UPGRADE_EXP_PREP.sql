create or replace package UPGRADE_EXP_PREP is

  -- Author  : Doug Durham
  -- Created : 3/25/2014 9:22:30 AM
  -- Purpose : Utility package for preparing an upgrade export
  -- This package was updated using the automation utility
  
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